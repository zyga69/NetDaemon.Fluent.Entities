using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// <see cref="IEntityInitializer"/> for <see cref="SwitchDefinition"/>: creates the HA <c>switch</c> entity,
    /// publishes its initial state, and subscribes to its MQTT command topic so toggling it in HA writes the
    /// new state back through MQTT and onto the bound device model property.
    /// </summary>
    /// <param name="_entityService">Creates/enables the underlying MQTT entity.</param>
    /// <param name="_entityManager">The NetDaemon MQTT entity manager used to subscribe to and publish switch state.</param>
    /// <param name="_converterFactory">Resolves the <see cref="ITypeConverter"/> for <see cref="bool"/>, used to render the initial state and parse incoming commands.</param>
    /// <param name="_serviceProvider">Resolves the device model instance the switch property is written onto.</param>
    /// <param name="logger">The Serilog logger.</param>
    internal class SwitchService(IEntityService _entityService, IMqttEntityManager _entityManager, IConverterFactory _converterFactory, IServiceProvider _serviceProvider, Serilog.ILogger logger) : ISwitchService
    {
        private readonly Serilog.ILogger _logger = logger.ForContext<SwitchService>();
        private readonly ITypeConverter _typeConverter = _converterFactory.GetService<bool>();

        /// <summary>Active command subscriptions, keyed by entity id, disposed in <see cref="Dispose"/>.</summary>
        protected readonly IDictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();

        /// <summary>
        /// Creates the switch entity, subscribes to its command topic, and publishes <see cref="SwitchDefinition.InitialState"/>.
        /// </summary>
        /// <param name="sensorDefinition">The entity definition; must be a <see cref="SwitchDefinition"/>.</param>
        /// <param name="deviceDefinition">The owning device definition, used to resolve the device model instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sensorDefinition"/> is not a <see cref="SwitchDefinition"/>.</exception>
        public async Task InitWithDevice(EntityDefinition sensorDefinition, DeviceDefinition deviceDefinition)
        {
            ArgumentNullException.ThrowIfNull(sensorDefinition);
            ArgumentNullException.ThrowIfNull(deviceDefinition);

            if (sensorDefinition is not SwitchDefinition switchDefinition)
                throw new ArgumentException($"Wrong entity type passed: [{sensorDefinition.GetType().Name}]");

            _logger.Debug($"Initializing with device: {switchDefinition}");

            await _entityService.Create(switchDefinition, deviceDefinition);

            _logger.Debug($"Subscribing [{switchDefinition.EntityId}]");

            var commandSubscription = await _entityManager
                .PrepareCommandSubscriptionAsync(switchDefinition.EntityId)
                .ConfigureAwait(false);

            var subscription = commandSubscription.Subscribe(async state =>
           {
               var convertedValue = _typeConverter.FromSensorString(state);
               if (convertedValue != null)
               {
                   var device = _serviceProvider.GetDevice(deviceDefinition);
                   switchDefinition.Property.SetValue(device, convertedValue);
               }

               await _entityManager.SetStateAsync(switchDefinition.EntityId, state);
           });

            _subscriptions.Add(switchDefinition.EntityId, subscription);

            string? initialState = _typeConverter.ToSensorString(switchDefinition.InitialState);
            if (initialState != null)
                await _entityManager.SetStateAsync(switchDefinition.EntityId, initialState);

            await _entityService.Enable(switchDefinition.EntityId);
        }

        /// <summary>Disposes all active command subscriptions.</summary>
        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                _logger.Debug($"Unsubscribing [{subscription.Key}]");
                subscription.Value?.Dispose();
            }
        }
    }
}
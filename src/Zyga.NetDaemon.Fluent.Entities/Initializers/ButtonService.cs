using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// <see cref="IEntityInitializer"/> for <see cref="ActionDefinition"/>: creates the HA <c>button</c> entity,
    /// and subscribes to its MQTT command topic so pressing it in HA invokes the mapped method on the device
    /// model instance. The entity is disabled for the duration of the invocation and re-enabled afterwards.
    /// </summary>
    internal class ButtonService : IButtonService
    {
        private readonly IEntityService _entityService;
        private readonly IMqttEntityManager _entityManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly Serilog.ILogger _logger;

        /// <summary>Active command subscriptions, keyed by entity id, disposed in <see cref="Dispose"/>.</summary>
        protected readonly IDictionary<string, IDisposable> _subscriptions = new Dictionary<string, IDisposable>();

        /// <summary>Creates the service with its collaborators.</summary>
        /// <param name="entityService">Creates/enables/disables the underlying MQTT entity.</param>
        /// <param name="entityManager">The NetDaemon MQTT entity manager used to subscribe to button presses.</param>
        /// <param name="serviceProvider">Resolves the device model instance the button method is invoked on.</param>
        /// <param name="logger">The Serilog logger.</param>
        public ButtonService(IEntityService entityService, IMqttEntityManager entityManager, IServiceProvider serviceProvider, Serilog.ILogger logger)
        {
            _entityService = entityService;
            _entityManager = entityManager;
            _serviceProvider = serviceProvider;
            _logger = logger.ForContext<ButtonService>();
        }

        /// <summary>
        /// Creates the button entity and subscribes to its command topic, invoking <see cref="ActionDefinition.Method"/>
        /// on the device model whenever the button is pressed in HA.
        /// </summary>
        /// <param name="sensorDefinition">The entity definition; must be an <see cref="ActionDefinition"/>.</param>
        /// <param name="deviceDefinition">The owning device definition, used to resolve the device model instance.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sensorDefinition"/> is not an <see cref="ActionDefinition"/>.</exception>
        public async Task InitWithDevice(EntityDefinition sensorDefinition, DeviceDefinition deviceDefinition)
        {
            ArgumentNullException.ThrowIfNull(sensorDefinition);
            ArgumentNullException.ThrowIfNull(deviceDefinition);

            if (sensorDefinition is not ActionDefinition actionDefinition)
                throw new ArgumentException($"Wrong entity type passed: [{sensorDefinition.GetType().Name}]");

            _logger.Debug($"Initializing with device: {actionDefinition}");

            await _entityService.Create(actionDefinition, deviceDefinition);

            _logger.Debug($"Subscribing [{actionDefinition.EntityId}]");

            var commandSubscription = await _entityManager
                .PrepareCommandSubscriptionAsync(actionDefinition.EntityId)
                .ConfigureAwait(false);

             var subscription = commandSubscription.Subscribe(async _ =>
            {
                await _entityService.Disable(actionDefinition.EntityId);

                var device = _serviceProvider.GetDevice(deviceDefinition);
                actionDefinition.Method.Invoke(device, null);

                await _entityService.Enable(actionDefinition.EntityId);
            });

            _subscriptions.Add(actionDefinition.EntityId, subscription);

            await _entityService.Enable(actionDefinition.EntityId);
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
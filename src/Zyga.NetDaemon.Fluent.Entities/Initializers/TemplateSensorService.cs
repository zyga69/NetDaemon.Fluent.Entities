using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;
using NetDaemon.Fluent.Entities.Model;
using Serilog;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Reflection.Metadata;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// <see cref="IEntityInitializer"/> for <see cref="TemplateSensorDefinition"/>. This is the push-based
    /// counterpart to <see cref="SensorService"/>: for each triggering entity, it sets an initial state from
    /// that entity's current state, then subscribes to <c>StateChanges()</c> and, on every change, invokes the
    /// mapped method on the device model and republishes its result as the sensor's HA state.
    /// </summary>
    /// <remarks>
    /// When <see cref="TemplateSensorDefinition.StableTimeSpan"/> is set, the triggering observable is debounced
    /// via <c>Observable.Timer(...).Switch()</c> on the injected <see cref="IScheduler"/> (rather than a plain
    /// <c>Throttle</c>/<c>Sample</c>), which is what makes the debounce testable with <c>TestScheduler</c> - each new
    /// state change restarts the timer, and only the last state that survives the full delay is applied. A
    /// computed value that is null or blank disables the entity (via <c>IEntityService.Disable</c>) instead of
    /// writing an empty state, so HA shows the entity as unavailable rather than blank.
    /// </remarks>
    internal class TemplateSensorService : ITemplateSensorService
    {
        private readonly IEntityService _entityService;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConverterFactory _converterFactory;
        private readonly IMqttEntityManager _entityManager;
        private readonly IScheduler _scheduler;

        /// <summary>Active state-change subscriptions (one per triggering entity), disposed in <see cref="Dispose"/>.</summary>
        protected readonly IList<IDisposable> _subscriptions = [];

        /// <summary>Creates the service with its collaborators.</summary>
        /// <param name="entityService">Creates/enables/disables the underlying MQTT entity.</param>
        /// <param name="logger">The Serilog logger.</param>
        /// <param name="serviceProvider">Resolves the device model instance the mapped method is invoked on.</param>
        /// <param name="converterFactory">Resolves the <see cref="ITypeConverter"/>s for the method's parameter and return types.</param>
        /// <param name="entityManager">The NetDaemon MQTT entity manager used to publish the computed state.</param>
        /// <param name="scheduler">The scheduler used to implement the <see cref="TemplateSensorDefinition.StableTimeSpan"/> debounce; injectable for testing.</param>
        public TemplateSensorService(IEntityService entityService, ILogger logger, IServiceProvider serviceProvider, IConverterFactory converterFactory, IMqttEntityManager entityManager, IScheduler scheduler)
        {
            _entityService = entityService;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _converterFactory = converterFactory;
            _entityManager = entityManager;
            _scheduler = scheduler;
        }

        /// <summary>
        /// Creates the template sensor entity, sets its initial state from the first triggering entity's current
        /// state, and subscribes to every triggering entity's state changes to keep it updated.
        /// </summary>
        /// <param name="sensorDefinition">The entity definition; must be a <see cref="TemplateSensorDefinition"/>.</param>
        /// <param name="deviceDefinition">The owning device definition, used to resolve the device model instance.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sensorDefinition"/> is not a <see cref="TemplateSensorDefinition"/>, or when
        /// its mapped method takes more than one parameter.
        /// </exception>
        public async Task InitWithDevice(EntityDefinition sensorDefinition, DeviceDefinition deviceDefinition)
        {
            ArgumentNullException.ThrowIfNull(sensorDefinition);
            ArgumentNullException.ThrowIfNull(deviceDefinition);

            if (sensorDefinition is not TemplateSensorDefinition templateSensorDefinition)
                throw new ArgumentException($"Wrong entity type passed: [{sensorDefinition.GetType().Name}]");

            var methodParameters = templateSensorDefinition.Method.GetParameters();
            if (methodParameters.Length > 1)
                throw new ArgumentException($"Passed method has more than one parameters");

            ParameterInfo? parameterInfo = methodParameters.Length == 1 ? methodParameters[0] : null;

            var resultTypeConverter = _converterFactory.GetService(templateSensorDefinition.Method.ReturnType);
            var device = _serviceProvider.GetDevice(deviceDefinition);

            _logger.Debug($"Initializing with device: {templateSensorDefinition}");
            await _entityService.Create(templateSensorDefinition, deviceDefinition);

            bool stateSet = false;

            foreach (var triggeringEntity in templateSensorDefinition.TriggeringEntities)
            {
                if (!stateSet)
                {
                    _logger.Debug($"Setiting initial state for [{templateSensorDefinition.EntityId}] from [{triggeringEntity.EntityId}][{triggeringEntity.State}]");
                    await UpdateState(triggeringEntity.State, triggeringEntity.EntityId, templateSensorDefinition, parameterInfo, device, resultTypeConverter);
                    stateSet = true;
                }

                
                IObservable<StateChange> observable = triggeringEntity.StateChanges();

                if (templateSensorDefinition.StableTimeSpan.HasValue)
                {
                    observable = observable
                        .Select(stateChange =>
                            Observable
                                .Timer(templateSensorDefinition.StableTimeSpan.Value, _scheduler)
                                .Select(_ => stateChange))
                        .Switch();
                }

                var subscription = observable.SubscribeSafe(async stateChange =>
                {
                    _logger.Debug($"Template Entity Triggered [{templateSensorDefinition.EntityId}] by [{stateChange.Entity.EntityId}] State: [{stateChange.Old?.State}] -> [{stateChange.New?.State}]");
                    await UpdateState(stateChange.New?.State, stateChange.Entity.EntityId, templateSensorDefinition, parameterInfo, device, resultTypeConverter);
                });
                _subscriptions.Add(subscription);
            }

            await _entityService.Enable(sensorDefinition.EntityId);
        }

        private async Task UpdateState(string? newState, string triggeringEntityId, TemplateSensorDefinition templateSensorDefinition, ParameterInfo? parameterInfo, object device, ITypeConverter resultTypeConverter)
        {
            bool canInvoke = true;
            object?[]? parameters = null;

            // If parameter info is null - this means that we have a method without arguments - still need to call it for evaluation
            if (parameterInfo != null)
            {
                var argTypeConverter = _converterFactory.GetService(parameterInfo.ParameterType);
                object? argSensorValue = argTypeConverter.FromSensorString(newState);


                if (argSensorValue == null && !parameterInfo.IsNullable())
                {
                    _logger.Warning($"Cannot determine current state of [{triggeringEntityId}]");
                    canInvoke = false;
                }

                parameters = [argSensorValue];
            }

            if (canInvoke)
            {
                string? sensorValue = null;

                object? deviceModelValue = templateSensorDefinition.Method.Invoke(device, parameters);  // parameters can be null here

                if (deviceModelValue != null)
                    sensorValue = resultTypeConverter.ToSensorString(deviceModelValue);

                if (!string.IsNullOrWhiteSpace(sensorValue))
                {
                    _logger.Verbose($"Setting [{sensorValue}] to [{templateSensorDefinition.EntityId}]");

                    await _entityService.Enable(templateSensorDefinition.EntityId).ConfigureAwait(false);

                    await _entityManager.SetStateAsync(templateSensorDefinition.EntityId, sensorValue)
                        .ConfigureAwait(false);
                } else
                {
                    await _entityService.Disable(templateSensorDefinition.EntityId).ConfigureAwait(false);
                }
            }
        }

        /// <summary>Disposes all active state-change subscriptions.</summary>
        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
        }
    }
}
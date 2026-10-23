using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// <see cref="IEntityInitializer"/> for <see cref="SensorDefinition"/>: creates the HA sensor entity and
    /// enables it. Unlike template sensors, values are not pushed reactively - they are written by
    /// <see cref="Update"/>, called from <c>IDeviceService.Update</c>/<c>UpdateFromModel</c>.
    /// </summary>
    internal class SensorService : ISensorService
    {
        private readonly IEntityService _entityService;
        private readonly IMqttEntityManager _mqttEntityManager;
        private readonly Serilog.ILogger _logger;
        private readonly IConverterFactory _converterFactory;

        /// <summary>Creates the service with its collaborators.</summary>
        /// <param name="entityService">Creates/enables the underlying MQTT entity.</param>
        /// <param name="mqttEntityManager">The NetDaemon MQTT entity manager used to publish sensor state.</param>
        /// <param name="logger">The Serilog logger.</param>
        /// <param name="converterFactory">Resolves the <see cref="ITypeConverter"/> for the sensor's property type.</param>
        public SensorService(IEntityService entityService, IMqttEntityManager mqttEntityManager, Serilog.ILogger logger, IConverterFactory converterFactory)
        {
            _entityService = entityService;
            _mqttEntityManager = mqttEntityManager;
            _logger = logger.ForContext<SensorService>();
            _converterFactory = converterFactory;
        }

        /// <summary>Creates and enables the HA sensor entity for a <see cref="SensorDefinition"/>.</summary>
        /// <param name="sensorDefinition">The entity definition; must be a <see cref="SensorDefinition"/>.</param>
        /// <param name="deviceDefinition">The owning device definition.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sensorDefinition"/> is not a <see cref="SensorDefinition"/>.</exception>
        public async Task InitWithDevice(EntityDefinition sensorDefinition, DeviceDefinition deviceDefinition)
        {
            ArgumentNullException.ThrowIfNull(sensorDefinition);
            ArgumentNullException.ThrowIfNull(deviceDefinition);
            
            if (sensorDefinition is not SensorDefinition)
                throw new ArgumentException($"Wrong entity type passed: [{sensorDefinition.GetType().Name}]");

            _logger.Debug($"Initializing with device: {sensorDefinition}");

            await _entityService.Create(sensorDefinition, deviceDefinition);
            await _entityService.Enable(sensorDefinition.EntityId);
        }

        /// <summary>
        /// Updates the device model property and publishes the resulting value as the sensor's HA state.
        /// When <paramref name="newValue"/> is supplied it is first converted and written onto <paramref name="property"/>
        /// (e.g. data arriving from an external source); otherwise the property's current value is read and published as-is.
        /// </summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="entityId">The HA entity id to publish the state to.</param>
        /// <param name="property">The device model property backing the sensor.</param>
        /// <param name="device">The device model instance.</param>
        /// <param name="newValue">The incoming raw value to convert and set on <paramref name="property"/>, or <c>null</c> to just republish the current value.</param>
        /// <returns>A task representing the asynchronous update.</returns>
        public async Task Update<TDevice>(string entityId, PropertyInfo property, TDevice device, string? newValue = null)
           where TDevice : class
        {
            ArgumentNullException.ThrowIfNull(entityId);
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(property);

            var typeConverter = _converterFactory.GetService(property.PropertyType);

            object? deviceModelValue;

            if (newValue != null)
            {
                deviceModelValue = typeConverter.FromDataPoint(newValue);
                property.SetValue(device, deviceModelValue, null);
            }
            else
            {
                deviceModelValue = property.GetValue(device);
            }

            if (deviceModelValue != null)
            {
                string? sensorValue = typeConverter.ToSensorString(deviceModelValue);

                if (sensorValue != null)
                    await _mqttEntityManager.SetStateAsync(entityId, sensorValue)
                        .ConfigureAwait(false);
            }
        }
    }
}
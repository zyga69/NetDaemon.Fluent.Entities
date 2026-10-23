using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities
{
    /// <inheritdoc/>
    internal class DeviceService : IDeviceService
    {
        private readonly ISensorService _sensorService;
        private readonly IButtonService _buttonService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEntityService _entityService;
        private readonly Serilog.ILogger _logger;
        private readonly IDeviceRegistry _deviceRegistry;
        private readonly IEntityInitializerFactory _entityInitializerFactory;

        /// <summary>Initializes a new instance of <see cref="DeviceService"/>.</summary>
        /// <param name="sensorService">Updates sensor entities for registered devices.</param>
        /// <param name="serviceProvider">Resolves device maps and device model instances from DI.</param>
        /// <param name="entityService">Enables/disables entities in HA.</param>
        /// <param name="logger">The Serilog logger, scoped to <see cref="DeviceService"/>.</param>
        /// <param name="deviceRegistry">Tracks which devices have been registered.</param>
        /// <param name="buttonService">Handles button entity initialization (unused directly, kept for DI parity with other initializers).</param>
        /// <param name="entityInitializerFactory">Resolves the <see cref="Initializers.IEntityInitializer"/> for each entity definition type.</param>
        public DeviceService(ISensorService sensorService, IServiceProvider serviceProvider, IEntityService entityService, Serilog.ILogger logger, IDeviceRegistry deviceRegistry, IButtonService buttonService, IEntityInitializerFactory entityInitializerFactory)
        {
            _sensorService = sensorService;
            _serviceProvider = serviceProvider;
            _entityService = entityService;
            _logger = logger.ForContext<DeviceService>();
            _deviceRegistry = deviceRegistry;
            _buttonService = buttonService;
            _entityInitializerFactory = entityInitializerFactory;
        }

        /// <inheritdoc/>
        public async Task Init<TMap>()
            where TMap : IDeviceMap
        {
            var map = _serviceProvider.GetService<TMap>();

            if (map == null)
                throw new Exception($"Map [{typeof(TMap).Name}] could not be found");

            await InitFromMap(map);
        }

        /// <inheritdoc/>
        public async Task InitFromMap<TMap>(TMap map)
            where TMap : IDeviceMap
        {
            if (map == null)
                throw new ArgumentNullException($"Map needs to be passed");

            var deviceDefinition = map.Definition;
            var deviceType = deviceDefinition.Type;

            // Skip Initialization if already in registry
            if (map.Definition.Key == null)
            {
                if (_deviceRegistry.Exists(deviceType))
                    return;
            }
            else
            {
                if (_deviceRegistry.ExistsKeyed(deviceType, map.Definition.Key))
                    return;
            }

            _logger.Debug($"Initializing: {deviceDefinition}");

            foreach (var entityDefinition in deviceDefinition.Entities)
            {
                var initializer = _entityInitializerFactory.GetService(entityDefinition.GetType());
                await initializer.InitWithDevice(entityDefinition, deviceDefinition);
            }

            _deviceRegistry.AddDefinition(deviceDefinition);
        }

        /// <inheritdoc/>
        public List<int> GetSensorIds<TDevice>(object key)
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.GetKeyed<TDevice>(key);

            return deviceDefinition.Sensors
                .Select(x => x.SensorId)
                .Where(x => x.HasValue)
                .Cast<int>()
                .ToList();
        }

        /// <inheritdoc/>
        public List<int> GetSensorIds<TDevice>()
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.Get<TDevice>();

            return deviceDefinition.Sensors
                .Select(x => x.SensorId)
                .Where(x => x.HasValue)
                .Cast<int>()
                .ToList();
        }

        /// <inheritdoc/>
        public async Task UpdateKeyed<TDevice>(object key, IEnumerable<IIdValue> dataPoints)
            where TDevice : class
        {
            var device = _serviceProvider.GetRequiredKeyedService<TDevice>(key);
            var deviceDefinition = _deviceRegistry.GetKeyed<TDevice>(key);

            await UpdateSensors(device, deviceDefinition, dataPoints);
        }

        /// <inheritdoc/>
        public async Task UpdateKeyedFromModel<TDevice>(object key, TDevice device)
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.GetKeyed<TDevice>(key);

            await UpdateSensors(device, deviceDefinition);
        }

        /// <inheritdoc/>
        public async Task Update<TDevice>(IEnumerable<IIdValue> dataPoints)
           where TDevice : class
        {
            var device = _serviceProvider.GetRequiredService<TDevice>();
            var deviceDefinition = _deviceRegistry.Get<TDevice>();

            await UpdateSensors(device, deviceDefinition, dataPoints);
        }

        /// <inheritdoc/>
        public async Task Update<TDevice>() where TDevice : class
        {
            var device = _serviceProvider.GetRequiredService<TDevice>();
            var deviceDefinition = _deviceRegistry.Get<TDevice>();

            await UpdateSensors(device, deviceDefinition);
        }

        /// <inheritdoc/>
        public async Task UpdateFromModel<TDevice>(TDevice device)
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.Get<TDevice>();

            await UpdateSensors(device, deviceDefinition);
        }

        /// <inheritdoc/>
        public async Task Disable<TDevice>()
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.Get<TDevice>();
            await Disable(deviceDefinition);
        }

        /// <inheritdoc/>
        public async Task DisableKeyed<TDevice>(object key)
          where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.GetKeyed<TDevice>(key);
            await Disable(deviceDefinition);
        }

        private async Task Disable(DeviceDefinition deviceDefinition)
        {
            _logger.Debug($"Disabling device: [{deviceDefinition.Identifier}]");

            deviceDefinition.State = DeviceState.Disabled;

            foreach (var entity in deviceDefinition.Entities)
                await _entityService.Disable(entity.EntityId);
        }

        /// <inheritdoc/>
        public async Task EnableKeyed<TDevice>(object key)
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.GetKeyed<TDevice>(key);
            await Enable(deviceDefinition);
        }

        /// <inheritdoc/>
        public async Task Enable<TDevice>()
          where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.Get<TDevice>();
            await Enable(deviceDefinition);
        }

        /// <inheritdoc/>
        public async Task EnsureEnabled<TDevice>()
            where TDevice : class
        {
            var deviceDefinition = _deviceRegistry.Get<TDevice>();
            if (deviceDefinition.State == DeviceState.Disabled)
                await Enable(deviceDefinition);
        }

        private async Task Enable(DeviceDefinition deviceDefinition)
        {
            _logger.Debug($"Enabling device: [{deviceDefinition.Identifier}]");

            deviceDefinition.State = DeviceState.Enabled;

            foreach (var entity in deviceDefinition.Entities)
                await _entityService.Enable(entity.EntityId);
        }

        private async Task UpdateSensors<TDevice>(TDevice device, DeviceDefinition deviceDefinition, IEnumerable<IIdValue> dataPoints)
            where TDevice : class
        {
            foreach (var sensorDefinition in deviceDefinition.Sensors)
            {
                var value = dataPoints.Where(x => x.Id == sensorDefinition.SensorId)
                    .FirstOrDefault()?
                    .Value;

                await _sensorService.Update(sensorDefinition.EntityId, sensorDefinition.Property, device, value);
            }
        }

        private async Task UpdateSensors<TDevice>(TDevice device, DeviceDefinition deviceDefinition)
            where TDevice : class
        {
            foreach (var sensorDefinition in deviceDefinition.Sensors)
                await _sensorService.Update(sensorDefinition.EntityId, sensorDefinition.Property, device);
        }
    }
}
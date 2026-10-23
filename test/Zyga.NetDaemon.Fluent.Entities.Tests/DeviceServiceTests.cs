using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class DeviceServiceTestDevice
    {
        public int Sensor { get; set; }
    }

    public class DeviceServiceTestMap : DeviceMap<DeviceServiceTestDevice>
    {
    }

    public class DeviceServiceTests
    {
        private readonly ISensorService _sensorService;
        private readonly IButtonService _buttonService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IKeyedServiceProvider _keyedServiceProvider;
        private readonly IEntityService _entityService;
        private readonly ILogger _logger;
        private readonly IDeviceRegistry _deviceRegistry;
        private readonly IEntityInitializerFactory _entityInitializerFactory;

        private readonly DeviceService _deviceService;

        public DeviceServiceTests()
        {
            _sensorService = Substitute.For<ISensorService>();
            _buttonService = Substitute.For<IButtonService>();
            _serviceProvider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
            _keyedServiceProvider = (IKeyedServiceProvider)_serviceProvider;
            _entityService = Substitute.For<IEntityService>();
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<DeviceService>().Returns(_logger);
            _deviceRegistry = Substitute.For<IDeviceRegistry>();
            _entityInitializerFactory = Substitute.For<IEntityInitializerFactory>();

            _deviceService = new DeviceService(
                _sensorService,
                _serviceProvider,
                _entityService,
                _logger,
                _deviceRegistry,
                _buttonService,
                _entityInitializerFactory);
        }

        /// <summary>Builds a map with a single device and a single property-backed sensor, mirroring the fluent API usage shown in DeviceMapTests.</summary>
        private static DeviceServiceTestMap BuildMap(object? key = null, int? sensorId = null)
        {
            var map = new DeviceServiceTestMap();

            if (key == null)
                map.AsDevice(StringHelpers.Random()).WithFriendlyName(StringHelpers.Random());
            else
                map.AsDevice(StringHelpers.Random(), key).WithFriendlyName(StringHelpers.Random());

            var root = map.Map(x => x.Sensor, $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}")
                .AsSensor();

            if (sensorId.HasValue)
                root.WithId(sensorId.Value);

            return map;
        }

        [Fact]
        public async Task Init_WhenMapCannotBeResolved_ThrowsException()
        {
            _serviceProvider.GetService(typeof(DeviceServiceTestMap)).Returns((object?)null);

            await Assert.ThrowsAsync<Exception>(() => _deviceService.Init<DeviceServiceTestMap>());
        }

        [Fact]
        public async Task Init_WhenMapResolved_InitializesEntitiesAndRegistersDevice()
        {
            var map = BuildMap();
            var deviceDefinition = map.Definition;
            var sensorDefinition = deviceDefinition.Sensors.Single();

            _serviceProvider.GetService(typeof(DeviceServiceTestMap)).Returns(map);
            _deviceRegistry.Exists(typeof(DeviceServiceTestDevice)).Returns(false);

            var initializer = Substitute.For<IEntityInitializer>();
            _entityInitializerFactory.GetService(sensorDefinition.GetType()).Returns(initializer);

            await _deviceService.Init<DeviceServiceTestMap>();

            await initializer.Received(1).InitWithDevice(sensorDefinition, deviceDefinition);
            _deviceRegistry.Received(1).AddDefinition(deviceDefinition);
        }

        [Fact]
        public async Task InitFromMap_WhenKeyedDeviceNotYetRegistered_InitializesEntitiesAndRegistersDevice()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key);
            var deviceDefinition = map.Definition;
            var sensorDefinition = deviceDefinition.Sensors.Single();

            _deviceRegistry.ExistsKeyed(typeof(DeviceServiceTestDevice), key).Returns(false);

            var initializer = Substitute.For<IEntityInitializer>();
            _entityInitializerFactory.GetService(sensorDefinition.GetType()).Returns(initializer);

            await _deviceService.InitFromMap(map);

            await initializer.Received(1).InitWithDevice(sensorDefinition, deviceDefinition);
            _deviceRegistry.Received(1).AddDefinition(deviceDefinition);
        }

        [Fact]
        public async Task InitFromMap_WithNullMap_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _deviceService.InitFromMap<IDeviceMap>(null!));
        }

        [Fact]
        public async Task InitFromMap_WhenDeviceAlreadyRegistered_SkipsInitialization()
        {
            var map = BuildMap();
            _deviceRegistry.Exists(typeof(DeviceServiceTestDevice)).Returns(true);

            await _deviceService.InitFromMap(map);

            _entityInitializerFactory.DidNotReceive().GetService(Arg.Any<Type>());
            _deviceRegistry.DidNotReceive().AddDefinition(Arg.Any<DeviceDefinition>());
        }

        [Fact]
        public async Task InitFromMap_WhenKeyedDeviceAlreadyRegistered_SkipsInitialization()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key);
            _deviceRegistry.ExistsKeyed(typeof(DeviceServiceTestDevice), key).Returns(true);

            await _deviceService.InitFromMap(map);

            _entityInitializerFactory.DidNotReceive().GetService(Arg.Any<Type>());
            _deviceRegistry.DidNotReceive().AddDefinition(Arg.Any<DeviceDefinition>());
        }

        [Fact]
        public void GetSensorIds_ReturnsOnlySensorsWithAnAssignedId()
        {
            var map = BuildMap(sensorId: 42);
            map.Map(x => x.Sensor, $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}").AsSensor();

            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            var result = _deviceService.GetSensorIds<DeviceServiceTestDevice>();

            Assert.Equal([42], result);
        }

        [Fact]
        public void GetSensorIds_Keyed_ReturnsOnlySensorsWithAnAssignedId()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key, sensorId: 11);

            _deviceRegistry.GetKeyed<DeviceServiceTestDevice>(key).Returns(map.Definition);

            var result = _deviceService.GetSensorIds<DeviceServiceTestDevice>(key);

            Assert.Equal([11], result);
        }

        [Fact]
        public async Task Update_WithDataPoints_PassesMatchingValueToSensorService()
        {
            var map = BuildMap(sensorId: 7);
            var sensorDefinition = map.Definition.Sensors.Single();
            var device = new DeviceServiceTestDevice();

            _serviceProvider.GetService(typeof(DeviceServiceTestDevice)).Returns(device);
            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            var dataPoint = Substitute.For<IIdValue>();
            dataPoint.Id.Returns(7);
            dataPoint.Value.Returns("123");

            await _deviceService.Update<DeviceServiceTestDevice>([dataPoint]);

            await _sensorService.Received(1)
                .Update(sensorDefinition.EntityId, sensorDefinition.Property, device, "123");
        }

        [Fact]
        public async Task Update_WithoutDataPoints_PushesCurrentDeviceValue()
        {
            var map = BuildMap();
            var device = new DeviceServiceTestDevice { Sensor = 99 };

            _serviceProvider.GetService(typeof(DeviceServiceTestDevice)).Returns(device);
            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            await _deviceService.Update<DeviceServiceTestDevice>();

            var sensorDefinition = map.Definition.Sensors.Single();
            await _sensorService.Received(1)
                .Update(sensorDefinition.EntityId, sensorDefinition.Property, device);
        }

        [Fact]
        public async Task UpdateFromModel_PushesGivenDeviceInstanceValues()
        {
            var map = BuildMap();
            var device = new DeviceServiceTestDevice { Sensor = 5 };

            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            await _deviceService.UpdateFromModel(device);

            var sensorDefinition = map.Definition.Sensors.Single();
            await _sensorService.Received(1)
                .Update(sensorDefinition.EntityId, sensorDefinition.Property, device);
        }

        [Fact]
        public async Task UpdateKeyed_PassesMatchingValueToSensorService()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key, sensorId: 3);
            var sensorDefinition = map.Definition.Sensors.Single();
            var device = new DeviceServiceTestDevice();

            _keyedServiceProvider.GetRequiredKeyedService(typeof(DeviceServiceTestDevice), key).Returns(device);
            _deviceRegistry.GetKeyed<DeviceServiceTestDevice>(key).Returns(map.Definition);

            var dataPoint = Substitute.For<IIdValue>();
            dataPoint.Id.Returns(3);
            dataPoint.Value.Returns("55");

            await _deviceService.UpdateKeyed<DeviceServiceTestDevice>(key, [dataPoint]);

            await _sensorService.Received(1)
                .Update(sensorDefinition.EntityId, sensorDefinition.Property, device, "55");
        }

        [Fact]
        public async Task UpdateKeyedFromModel_PushesGivenDeviceInstanceValues()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key);
            var device = new DeviceServiceTestDevice { Sensor = 8 };

            _deviceRegistry.GetKeyed<DeviceServiceTestDevice>(key).Returns(map.Definition);

            await _deviceService.UpdateKeyedFromModel(key, device);

            var sensorDefinition = map.Definition.Sensors.Single();
            await _sensorService.Received(1)
                .Update(sensorDefinition.EntityId, sensorDefinition.Property, device);
        }

        [Fact]
        public async Task Disable_MarksDeviceDisabledAndDisablesEveryEntity()
        {
            var map = BuildMap();
            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            await _deviceService.Disable<DeviceServiceTestDevice>();

            Assert.Equal(DeviceState.Disabled, map.Definition.State);
            var sensorDefinition = map.Definition.Sensors.Single();
            await _entityService.Received(1).Disable(sensorDefinition.EntityId);
        }

        [Fact]
        public async Task DisableKeyed_MarksDeviceDisabledAndDisablesEveryEntity()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key);
            _deviceRegistry.GetKeyed<DeviceServiceTestDevice>(key).Returns(map.Definition);

            await _deviceService.DisableKeyed<DeviceServiceTestDevice>(key);

            Assert.Equal(DeviceState.Disabled, map.Definition.State);
            var sensorDefinition = map.Definition.Sensors.Single();
            await _entityService.Received(1).Disable(sensorDefinition.EntityId);
        }

        [Fact]
        public async Task Enable_MarksDeviceEnabledAndEnablesEveryEntity()
        {
            var map = BuildMap();
            map.Definition.State = DeviceState.Disabled;
            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            await _deviceService.Enable<DeviceServiceTestDevice>();

            Assert.Equal(DeviceState.Enabled, map.Definition.State);
            var sensorDefinition = map.Definition.Sensors.Single();
            await _entityService.Received(1).Enable(sensorDefinition.EntityId);
        }

        [Fact]
        public async Task EnableKeyed_MarksDeviceEnabledAndEnablesEveryEntity()
        {
            var key = StringHelpers.Random();
            var map = BuildMap(key: key);
            map.Definition.State = DeviceState.Disabled;
            _deviceRegistry.GetKeyed<DeviceServiceTestDevice>(key).Returns(map.Definition);

            await _deviceService.EnableKeyed<DeviceServiceTestDevice>(key);

            Assert.Equal(DeviceState.Enabled, map.Definition.State);
            var sensorDefinition = map.Definition.Sensors.Single();
            await _entityService.Received(1).Enable(sensorDefinition.EntityId);
        }

        [Fact]
        public async Task EnsureEnabled_WhenDisabled_EnablesDevice()
        {
            var map = BuildMap();
            map.Definition.State = DeviceState.Disabled;
            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            await _deviceService.EnsureEnabled<DeviceServiceTestDevice>();

            Assert.Equal(DeviceState.Enabled, map.Definition.State);
            var sensorDefinition = map.Definition.Sensors.Single();
            await _entityService.Received(1).Enable(sensorDefinition.EntityId);
        }

        [Fact]
        public async Task EnsureEnabled_WhenAlreadyEnabled_DoesNothing()
        {
            var map = BuildMap();
            map.Definition.State = DeviceState.Enabled;
            _deviceRegistry.Get<DeviceServiceTestDevice>().Returns(map.Definition);

            await _deviceService.EnsureEnabled<DeviceServiceTestDevice>();

            Assert.Equal(DeviceState.Enabled, map.Definition.State);
            await _entityService.DidNotReceive().Enable(Arg.Any<string>());
        }
    }
}

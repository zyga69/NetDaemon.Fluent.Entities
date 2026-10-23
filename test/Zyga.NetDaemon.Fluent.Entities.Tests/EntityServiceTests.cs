using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Model.EntityCreation;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class EntityServiceTestDevice
    {
        public int Sensor { get; set; }
    }

    public class EntityServiceTests
    {
        private readonly IMqttEntityManager _mqttEntityManager;
        private readonly ILogger _logger;

        private readonly EntityService _entityService;

        public EntityServiceTests()
        {
            _mqttEntityManager = Substitute.For<IMqttEntityManager>();
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<EntityService>().Returns(_logger);

            _entityService = new EntityService(_mqttEntityManager, _logger);
        }

        private static SensorDefinition BuildSensorDefinition(string? friendlyName = null)
        {
            var property = typeof(EntityServiceTestDevice).GetProperty(nameof(EntityServiceTestDevice.Sensor));
            ArgumentNullException.ThrowIfNull(property);

            string deviceId = StringHelpers.Random();
            string entityId = $"sensor.{deviceId}_{StringHelpers.Random()}";

            var attributes = friendlyName == null ? null : new EntityAttributes { FriendlyName = friendlyName };

            return new SensorDefinition(entityId, "sensor", deviceId, property, attributes);
        }

        [Fact]
        public async Task Create_WithoutDeviceDefinition_CreatesEntityUsingExpectedOptions()
        {
            string friendlyName = StringHelpers.Random();
            var sensorDefinition = BuildSensorDefinition(friendlyName);
            sensorDefinition.DeviceClass = StringHelpers.Random();

            await _entityService.Create(sensorDefinition);

            await _mqttEntityManager.Received(1).CreateAsync(
                sensorDefinition.EntityId,
                Arg.Is<EntityCreationOptions>(o =>
                    o.Name == friendlyName &&
                    o.DeviceClass == sensorDefinition.DeviceClass &&
                    o.PayloadAvailable == EntityService.AVAILABLE &&
                    o.PayloadNotAvailable == EntityService.NOT_AVAILABLE),
                Arg.Any<object>());
        }

        [Fact]
        public async Task Create_WithDeviceDefinition_IncludesDeviceInAdditionalConfig()
        {
            var sensorDefinition = BuildSensorDefinition();
            var deviceDefinition = new DeviceDefinition(StringHelpers.Random(), typeof(EntityServiceTestDevice))
            {
                FriendlyName = StringHelpers.Random()
            };

            object? capturedConfig = null;
            _mqttEntityManager.CreateAsync(
                sensorDefinition.EntityId,
                Arg.Any<EntityCreationOptions>(),
                Arg.Do<object>(c => capturedConfig = c))
                .Returns(Task.CompletedTask);

            await _entityService.Create(sensorDefinition, deviceDefinition);

            var dict = Assert.IsAssignableFrom<IDictionary<string, object?>>(capturedConfig);
            var deviceConfig = Assert.IsType<DeviceConfig>(dict["device"]);
            Assert.Equal(deviceDefinition.Identifier, deviceConfig.identifiers.Single());
            Assert.Equal(deviceDefinition.FriendlyName, deviceConfig.name);
        }

        [Fact]
        public async Task Enable_SetsEntityAvailable()
        {
            string entityId = StringHelpers.Random();

            await _entityService.Enable(entityId);

            await _mqttEntityManager.Received(1).SetAvailabilityAsync(entityId, EntityService.AVAILABLE);
        }

        [Fact]
        public async Task Disable_SetsEntityNotAvailable()
        {
            string entityId = StringHelpers.Random();

            await _entityService.Disable(entityId);

            await _mqttEntityManager.Received(1).SetAvailabilityAsync(entityId, EntityService.NOT_AVAILABLE);
        }

        [Fact]
        public async Task DisposeAsync_DisablesEveryPreviouslyCreatedEntity()
        {
            var sensor1 = BuildSensorDefinition();
            var sensor2 = BuildSensorDefinition();

            await _entityService.Create(sensor1);
            await _entityService.Create(sensor2);

            await _entityService.DisposeAsync();

            await _mqttEntityManager.Received(1).SetAvailabilityAsync(sensor1.EntityId, EntityService.NOT_AVAILABLE);
            await _mqttEntityManager.Received(1).SetAvailabilityAsync(sensor2.EntityId, EntityService.NOT_AVAILABLE);
        }
    }
}

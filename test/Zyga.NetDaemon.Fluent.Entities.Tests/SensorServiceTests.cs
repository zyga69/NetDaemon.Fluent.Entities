using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NSubstitute;
using Serilog;
using System.Reflection;
using System.Security.Cryptography;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class TestDevice
    {
        public int IntSensor { get; set; }
        public double DoubleSensor { get; set; }
        public string StringSensor { get; set; } = string.Empty;
        public DateTime DateTimeSensor { get; set; }
        public bool BoolSensor { get; set; }
    }

    public class SensorServiceTests
    {
        private readonly IEntityService _entityService;
        private readonly IMqttEntityManager _mqttEntityManager;
        private readonly ILogger _logger;
        private readonly IConverterFactory _converterFactory;

        private readonly ISensorService _sensorService;

        public SensorServiceTests()
        {
            _entityService = Substitute.For<IEntityService>();
            _mqttEntityManager = Substitute.For<IMqttEntityManager>();
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<SensorService>().Returns(_logger);

            _converterFactory = Substitute.For<IConverterFactory>();

            _sensorService = new SensorService(_entityService, _mqttEntityManager, _logger, _converterFactory);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Sensor_Update(bool passValue)
        {
            string entityId = StringHelpers.Random();
            PropertyInfo? property = typeof(TestDevice).GetProperty(nameof(TestDevice.IntSensor));
            ArgumentNullException.ThrowIfNull(property);

            int value = RandomNumberGenerator.GetInt32(0, 100);
            string stringvalue = value.ToString();

            ITypeConverter typeConverter = Substitute.For<ITypeConverter>();
            typeConverter.FromDataPoint(stringvalue).Returns(value);
            typeConverter.ToSensorString(value).Returns(stringvalue);

            _converterFactory.GetService(typeof(int))
                .Returns(typeConverter);

            var device = new TestDevice();
            if (passValue)
            {
                await _sensorService.Update(entityId, property, device, stringvalue);
                Assert.Equal(value, device.IntSensor);
            }
            else
            {
                device.IntSensor = value;
                await _sensorService.Update(entityId, property, device);
            }

            await _mqttEntityManager.Received()
                .SetStateAsync(Arg.Is(entityId), Arg.Is(device.IntSensor.ToString()));
        }

        private static SensorDefinition BuildSensorDefinition()
        {
            var property = typeof(TestDevice).GetProperty(nameof(TestDevice.IntSensor));
            ArgumentNullException.ThrowIfNull(property);

            string deviceId = StringHelpers.Random();
            string entityId = $"sensor.{deviceId}_{StringHelpers.Random()}";

            return new SensorDefinition(entityId, "sensor", deviceId, property);
        }

        [Fact]
        public async Task InitWithDevice_WithNullDefinition_ThrowsArgumentNullException()
        {
            var deviceDefinition = new DeviceDefinition(StringHelpers.Random(), typeof(TestDevice));

            await Assert.ThrowsAsync<ArgumentNullException>(() => _sensorService.InitWithDevice(null!, deviceDefinition));
        }

        [Fact]
        public async Task InitWithDevice_WithNullDeviceDefinition_ThrowsArgumentNullException()
        {
            var sensorDefinition = BuildSensorDefinition();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _sensorService.InitWithDevice(sensorDefinition, null!));
        }

        [Fact]
        public async Task InitWithDevice_WithWrongDefinitionType_ThrowsArgumentException()
        {
            var sensorDefinition = BuildSensorDefinition();
            var deviceDefinition = new DeviceDefinition(sensorDefinition.DeviceClass ?? StringHelpers.Random(), typeof(TestDevice));

            var method = typeof(TestDevice).GetMethod(nameof(object.ToString));
            ArgumentNullException.ThrowIfNull(method);
            var wrongDefinition = new ActionDefinition(StringHelpers.Random(), "button", deviceDefinition.Identifier, method);

            await Assert.ThrowsAsync<ArgumentException>(() => _sensorService.InitWithDevice(wrongDefinition, deviceDefinition));
        }

        [Fact]
        public async Task InitWithDevice_WithSensorDefinition_CreatesAndEnablesEntity()
        {
            var sensorDefinition = BuildSensorDefinition();
            var deviceDefinition = new DeviceDefinition(StringHelpers.Random(), typeof(TestDevice));

            await _sensorService.InitWithDevice(sensorDefinition, deviceDefinition);

            await _entityService.Received(1).Create(sensorDefinition, deviceDefinition);
            await _entityService.Received(1).Enable(sensorDefinition.EntityId);
        }
    }
}
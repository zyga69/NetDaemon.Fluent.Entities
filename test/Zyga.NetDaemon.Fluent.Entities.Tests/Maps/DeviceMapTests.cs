using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Tests.Helpers;

namespace NetDaemon.Fluent.Entities.Tests.Maps
{
    public class TestDevice
    {
        public required int Sensor { get; set; }
        public required DateTime DateTime { get; set; }
        public bool? BinarySensor { get; set; }
    }

    public class TestMap : DeviceMap<TestDevice>
    {
        public void InitDevice()
        {
            AsDevice(StringHelpers.Random())
                .WithFriendlyName(StringHelpers.Random());




        }
    }

    public enum TestEnum
    {
        AbcDff,
        Gwefeenw
    }

    public class DeviceMapTests
    {
        string sensorPrefix = "sensor";

        [Fact]
        public void Device_Normal()
        {
            string deviceName = StringHelpers.Random();
            string deviceFriendlyName = StringHelpers.Random();

            var sensorMap = new TestMap();

            sensorMap.AsDevice(deviceName)
                .WithFriendlyName(deviceFriendlyName);

            var definition = sensorMap.Definition;

            Assert.Equal(deviceName, definition.Identifier);
            Assert.Equal(deviceFriendlyName, definition.FriendlyName);
            Assert.Null(definition.Key);
        }

        [Fact]
        public void Device_Keyed_String()
        {
            string deviceName = StringHelpers.Random();
            string deviceFriendlyName = StringHelpers.Random();
            string key = StringHelpers.Random();

            var sensorMap = new TestMap();

            sensorMap.AsDevice(deviceName, key)
                .WithFriendlyName(deviceFriendlyName);

            var definition = sensorMap.Definition;

            Assert.Equal(deviceName, definition.Identifier);
            Assert.Equal(deviceFriendlyName, definition.FriendlyName);
            Assert.Equal(key, definition.Key);
        }

        [Fact]
        public void Device_Keyed_Enum()
        {
            string deviceName = StringHelpers.Random();
            string deviceFriendlyName = StringHelpers.Random();
            var key = TestEnum.Gwefeenw;

            var sensorMap = new TestMap();

            sensorMap.AsDevice(deviceName, key)
                .WithFriendlyName(deviceFriendlyName);

            var definition = sensorMap.Definition;

            Assert.Equal(deviceName, definition.Identifier);
            Assert.Equal(deviceFriendlyName, definition.FriendlyName);
            Assert.Equal(key, definition.Key);
        }

        [Fact]
        public void Sensor_NoDevice()
        {
            var sensorMap = new TestMap();

            Assert.Throws<InvalidOperationException>(() =>
            {
                sensorMap.Map(x => x.Sensor, new Guid().ToString());
            });
        }

        [Fact]
        public void Sensor_Generic()
        {
            var sensorMap = new TestMap();
            sensorMap.InitDevice();

            string tempSensorName = $"{sensorPrefix}.{sensorMap.Definition.Identifier}_{StringHelpers.Random()}";

            sensorMap.Map(x => x.Sensor, tempSensorName)
                .AsSensor();

            var definition = sensorMap.Definition;

            Assert.Single(definition.Sensors);

            var tempSensor = definition.Sensors.Single();
            Assert.Equal(tempSensorName, tempSensor.EntityId);
            Assert.StartsWith(sensorPrefix, tempSensor.EntityId);
        }

        [Fact]
        public void Sensor_Generic_WithId()
        {

            int sensorId = (int)DateTime.Now.Ticks;

            var sensorMap = new TestMap();
            sensorMap.InitDevice();

            string tempSensorName = $"{sensorPrefix}.{sensorMap.Definition.Identifier}_{StringHelpers.Random()}";

            sensorMap.Map(x => x.Sensor, tempSensorName)
                .AsSensor()
                .WithId(sensorId);

            var definition = sensorMap.Definition;

            Assert.Single(definition.Sensors);

            var tempSensor = definition.Sensors.Single();
            Assert.Equal(tempSensorName, tempSensor.EntityId);
            Assert.StartsWith(sensorPrefix, tempSensor.EntityId);

            Assert.Equal(sensorId, tempSensor.SensorId);
        }


        [Fact]
        public void Sensor_Generic_WithFriendlyName()
        {

            string tempSensorFriendlyName = StringHelpers.Random();

            var sensorMap = new TestMap();
            sensorMap.InitDevice();

            string tempSensorName = $"{sensorPrefix}.{sensorMap.Definition.Identifier}_{StringHelpers.Random()}";

            sensorMap.Map(x => x.Sensor, tempSensorName)
                .WithFriendlyName(tempSensorFriendlyName)
                .AsSensor();

            var definition = sensorMap.Definition;

            Assert.Single(definition.Sensors);

            var tempSensor = definition.Sensors.Single();
            Assert.Equal(tempSensorName, tempSensor.EntityId);
            Assert.StartsWith(sensorPrefix, tempSensor.EntityId);

            Assert.Equal(tempSensorFriendlyName, tempSensor.FriendlyName);
        }
        // las palmas byslawkou
        // lesny oddech
        // krzyzaleski
        //

        [Fact]
        public void Sensor_Temperature()
        {
            string tempSensorFriendlyName = StringHelpers.Random();

            var sensorMap = new TestMap();
            sensorMap.InitDevice();

            string tempSensorName = $"{sensorPrefix}.{sensorMap.Definition.Identifier}_{StringHelpers.Random()}";

            sensorMap.Map(x => x.Sensor, tempSensorName)
                .WithFriendlyName(tempSensorFriendlyName)
                .AsTemperatureSensor();


            var definition = sensorMap.Definition;

            Assert.Single(definition.Sensors);

            var tempSensor = definition.Sensors.Single();
            Assert.Equal(tempSensorName, tempSensor.EntityId);
            Assert.StartsWith(sensorPrefix, tempSensor.EntityId);

            Assert.Equal(tempSensorFriendlyName, tempSensor.FriendlyName);
            Assert.Equal(DeviceClass.Temperature.GetDescription(), tempSensor.DeviceClass);
            Assert.Equal(UnitOfMeasurement.Celsius.GetDescription(), tempSensor.UnitOfMeasurement);
        }

        [Fact]
        public void Sensor_Timestamp()
        {
            string tempSensorFriendlyName = StringHelpers.Random();

            var sensorMap = new TestMap();
            sensorMap.InitDevice();

            string tempSensorName = $"{sensorPrefix}.{sensorMap.Definition.Identifier}_{StringHelpers.Random()}";

            sensorMap.Map(x => x.DateTime, tempSensorName)
                .WithFriendlyName(tempSensorFriendlyName)
                .AsTimestamp();

            var definition = sensorMap.Definition;

            Assert.Single(definition.Sensors);

            var tempSensor = definition.Sensors.Single();
            Assert.Equal(tempSensorName, tempSensor.EntityId);
            Assert.StartsWith(sensorPrefix, tempSensor.EntityId);

            Assert.Equal(tempSensorFriendlyName, tempSensor.FriendlyName);
            Assert.Equal(DeviceClass.Timestamp.GetDescription(), tempSensor.DeviceClass);
        }

        [Fact]
        public void Sensor_BinarySensor()
        {
            string tempSensorFriendlyName = StringHelpers.Random();
            string binarySensorPrefix = "binary_sensor";

            var sensorMap = new TestMap();
            sensorMap.InitDevice();

            string tempSensorName = $"{binarySensorPrefix}.{sensorMap.Definition.Identifier}_{StringHelpers.Random()}";

            sensorMap.Map(x => x.BinarySensor, tempSensorName)
                .WithFriendlyName(tempSensorFriendlyName)
                .AsBinarySensor();

            var definition = sensorMap.Definition;

            Assert.Single(definition.Sensors);

            var binarySensor = definition.Sensors.Single();
            Assert.Equal(tempSensorName, binarySensor.EntityId);
            Assert.StartsWith(binarySensorPrefix, binarySensor.EntityId);
            Assert.Equal(tempSensorFriendlyName, binarySensor.FriendlyName);
        }
    }
}
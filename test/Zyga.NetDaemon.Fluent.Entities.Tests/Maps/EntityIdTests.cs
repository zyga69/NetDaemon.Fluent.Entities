using NetDaemon.Fluent.Entities.Maps;

namespace NetDaemon.Fluent.Entities.Tests.Maps
{
    public class EntityIdTests
    {
        [Fact]
        public void EmptyString()
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2";

            string toBeParsed = string.Empty;

            Assert.Throws<InvalidDataException>(() =>
            {
                var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);
            });
        }

        [Fact]
        public void EntityName()
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2";
            string entityName = "heat_1";

            string toBeParsed = entityName;

            var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);

            Assert.Equal($"{prefix}.{deviceId}_{entityName}", entityId.ToString());
        }

        [Theory]
        [InlineData("sensor.asdasd_A_aaa")]
        [InlineData("sensor.aaa@")]
        [InlineData("sensor.aaa#")]
        public void EntityName_WrongCharacters(string toBeParsed)
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2";

            Assert.Throws<InvalidDataException>(() =>
            {
                var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);
            });
        }

        [Fact]
        public void DeviceAndEntityName()
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2";
            string entityName = "heat_1";

            string toBeParsed = $"{deviceId}_{entityName}";

            var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);

            Assert.Equal($"{prefix}.{deviceId}_{entityName}", entityId.ToString());
        }

        [Fact]
        public void PrefixDeviceAndEntityName()
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2";
            string entityName = "heat_1";

            string toBeParsed = $"{prefix}.{deviceId}_{entityName}";

            var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);

            Assert.Equal($"{prefix}.{deviceId}_{entityName}", entityId.ToString());
        }

        [Fact]
        public void PrefixDeviceAndEntityName_WrongPrefix()
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2";
            string entityName = "heat_1";

            string toBeParsed = $"binary_sensor.{deviceId}_{entityName}";

            Assert.Throws<InvalidDataException>(() =>
            {
                var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);
            });
        }

        [Fact]
        public void PrefixDeviceAndEntityName_ToManyDots()
        {
            string prefix = "sensor";
            string deviceId = "boiler_nr2.3";
            string entityName = "heat_1";

            string toBeParsed = $"{prefix}.{deviceId}_{entityName}";

            Assert.Throws<InvalidDataException>(() =>
            {
                var entityId = ParsedEntityId.Parse(toBeParsed, prefix, deviceId);
            });
        }
    }
}
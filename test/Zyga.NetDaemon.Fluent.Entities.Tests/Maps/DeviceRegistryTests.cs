using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Tests.Helpers;

namespace NetDaemon.Fluent.Entities.Tests.Maps
{
    public class DeviceRegistryTests
    {
        private class RegistryTestDevice
        {
        }

        [Fact]
        public void ExistsKeyed_RegisteredKey_ReturnsTrue()
        {
            string key = StringHelpers.Random();

            var definition = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice))
            {
                Key = key
            };

            var registry = new DeviceRegistry();
            registry.AddDefinition(definition);

            var result = registry.ExistsKeyed(typeof(RegistryTestDevice), key);

            Assert.True(result);
        }

        [Fact]
        public void ExistsKeyed_UnregisteredKey_ReturnsFalse()
        {
            string registeredKey = StringHelpers.Random();
            string otherKey = StringHelpers.Random();

            var definition = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice))
            {
                Key = registeredKey
            };

            var registry = new DeviceRegistry();
            registry.AddDefinition(definition);

            var result = registry.ExistsKeyed(typeof(RegistryTestDevice), otherKey);

            Assert.False(result);
        }
    }
}

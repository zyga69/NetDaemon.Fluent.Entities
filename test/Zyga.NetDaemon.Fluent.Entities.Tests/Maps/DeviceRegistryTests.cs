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

        [Fact]
        public void ExistsKeyed_UnregisteredType_ReturnsFalse()
        {
            var registry = new DeviceRegistry();

            var result = registry.ExistsKeyed(typeof(RegistryTestDevice), StringHelpers.Random());

            Assert.False(result);
        }

        [Fact]
        public void Exists_RegisteredType_ReturnsTrue()
        {
            var definition = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice));

            var registry = new DeviceRegistry();
            registry.AddDefinition(definition);

            Assert.True(registry.Exists(typeof(RegistryTestDevice)));
        }

        [Fact]
        public void Exists_UnregisteredType_ReturnsFalse()
        {
            var registry = new DeviceRegistry();

            Assert.False(registry.Exists(typeof(RegistryTestDevice)));
        }

        [Fact]
        public void Get_RegisteredType_ReturnsSingleDefinition()
        {
            var definition = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice));

            var registry = new DeviceRegistry();
            registry.AddDefinition(definition);

            var result = registry.Get<RegistryTestDevice>();

            Assert.Same(definition, result);
        }

        [Fact]
        public void Get_UnregisteredType_Throws()
        {
            var registry = new DeviceRegistry();

            Assert.Throws<Exception>(() => registry.Get<RegistryTestDevice>());
        }

        [Fact]
        public void GetKeyed_RegisteredKey_ReturnsMatchingDefinition()
        {
            string key = StringHelpers.Random();

            var other = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice))
            {
                Key = StringHelpers.Random()
            };
            var matching = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice))
            {
                Key = key
            };

            var registry = new DeviceRegistry();
            registry.AddDefinition(other);
            registry.AddDefinition(matching);

            var result = registry.GetKeyed<RegistryTestDevice>(key);

            Assert.Same(matching, result);
        }

        [Fact]
        public void GetKeyed_UnregisteredType_Throws()
        {
            var registry = new DeviceRegistry();

            Assert.Throws<Exception>(() => registry.GetKeyed<RegistryTestDevice>(StringHelpers.Random()));
        }

        [Fact]
        public void GetKeyed_UnregisteredKey_Throws()
        {
            var definition = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice))
            {
                Key = StringHelpers.Random()
            };

            var registry = new DeviceRegistry();
            registry.AddDefinition(definition);

            Assert.Throws<Exception>(() => registry.GetKeyed<RegistryTestDevice>(StringHelpers.Random()));
        }

        [Fact]
        public void GetKeyed_NullKey_Throws()
        {
            var registry = new DeviceRegistry();

            Assert.Throws<ArgumentNullException>(() => registry.GetKeyed<RegistryTestDevice>(null!));
        }

        [Fact]
        public void AddDefinition_SecondDefinitionForSameType_AppendsRatherThanReplaces()
        {
            var first = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice));
            var second = new DeviceDefinition(StringHelpers.Random(), typeof(RegistryTestDevice));

            var registry = new DeviceRegistry();
            registry.AddDefinition(first);
            registry.AddDefinition(second);

            Assert.Equal(2, registry[typeof(RegistryTestDevice)].Count);
        }
    }
}

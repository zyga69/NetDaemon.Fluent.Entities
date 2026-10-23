using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class RegistrationExtensionsTests
    {
        [Theory]
        [InlineData(typeof(TemplateSensorDefinition), typeof(TemplateSensorService))]
        [InlineData(typeof(SensorDefinition), typeof(SensorService))]
        [InlineData(typeof(ActionDefinition), typeof(ButtonService))]
        [InlineData(typeof(SwitchDefinition), typeof(SwitchService))]
        public void AddEntityInitializers_RegistersExpectedImplementationKeyedByDefinitionType(Type key, Type expectedImplementation)
        {
            var services = new ServiceCollection();

            services.AddEntityInitializers();

            var descriptor = Assert.Single(services, d =>
                d.ServiceType == typeof(IEntityInitializer) &&
                Equals(d.ServiceKey, key));

            Assert.Equal(expectedImplementation, descriptor.KeyedImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void AddEntityInitializers_ReturnsSameServiceCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddEntityInitializers();

            Assert.Same(services, result);
        }
    }
}

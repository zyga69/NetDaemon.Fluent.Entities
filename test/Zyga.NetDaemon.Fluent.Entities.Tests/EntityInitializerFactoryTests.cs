using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NSubstitute;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class EntityInitializerFactoryTests
    {
        [Fact]
        public void GetService_ResolvesRegisteredInitializerForDefinitionType()
        {
            var initializer = Substitute.For<IEntityInitializer>();
            var serviceProvider = Substitute.For<IServiceProvider, IKeyedServiceProvider>();
            var keyedServiceProvider = (IKeyedServiceProvider)serviceProvider;
            keyedServiceProvider.GetKeyedService(typeof(IEntityInitializer), typeof(SensorDefinition)).Returns(initializer);

            var factory = new EntityInitializerFactory(serviceProvider);

            var result = factory.GetService(typeof(SensorDefinition));

            Assert.Same(initializer, result);
        }
    }
}

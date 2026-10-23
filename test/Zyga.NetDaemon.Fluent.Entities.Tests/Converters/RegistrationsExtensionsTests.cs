using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Converters;

namespace NetDaemon.Fluent.Entities.Tests.Converters
{
    public class RegistrationsExtensionsTests
    {
        [Theory]
        [InlineData(typeof(int), typeof(IntConverter))]
        [InlineData(typeof(double), typeof(DoubleConverter))]
        [InlineData(typeof(string), typeof(StringConverter))]
        [InlineData(typeof(bool), typeof(BooleanConverter))]
        [InlineData(typeof(DateTime), typeof(DateTimeConverter))]
        public void AddTypeConverters_RegistersExpectedImplementationKeyedByClrType(Type key, Type expectedImplementation)
        {
            var services = new ServiceCollection();

            services.AddTypeConverters();

            var descriptor = Assert.Single(services, d =>
                d.ServiceType == typeof(ITypeConverter) &&
                Equals(d.ServiceKey, key));

            Assert.Equal(expectedImplementation, descriptor.KeyedImplementationType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void AddTypeConverters_ReturnsSameServiceCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddTypeConverters();

            Assert.Same(services, result);
        }
    }
}

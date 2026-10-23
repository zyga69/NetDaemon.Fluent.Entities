using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Converters;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests.Converters
{
    public class ConverterFactoryTests
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConverterFactory _factory;

        public ConverterFactoryTests()
        {
            var logger = Substitute.For<ILogger>();

            var services = new ServiceCollection();
            services.AddSingleton(logger);
            services.AddTypeConverters();
            _serviceProvider = services.BuildServiceProvider();

            _factory = new ConverterFactory(_serviceProvider);
        }

        [Fact]
        public void GetService_RegisteredType_ReturnsExpectedConverter()
        {
            var result = _factory.GetService(typeof(int));

            Assert.IsType<IntConverter>(result);
        }

        [Fact]
        public void GetServiceOfT_RegisteredType_ReturnsExpectedConverter()
        {
            var result = _factory.GetService<string>();

            Assert.IsType<StringConverter>(result);
        }

        [Fact]
        public void GetService_NullableUnderlyingTypeRegistered_ResolvesUnderlyingConverter()
        {
            var result = _factory.GetService(typeof(bool?));

            Assert.IsType<BooleanConverter>(result);
        }

        [Fact]
        public void GetService_CalledTwice_ReturnsSameCachedInstance()
        {
            var first = _factory.GetService(typeof(double));
            var second = _factory.GetService(typeof(double));

            Assert.Same(first, second);
        }

        [Fact]
        public void GetService_UnregisteredType_Throws()
        {
            Assert.Throws<Exception>(() => _factory.GetService(typeof(Guid)));
        }
    }
}

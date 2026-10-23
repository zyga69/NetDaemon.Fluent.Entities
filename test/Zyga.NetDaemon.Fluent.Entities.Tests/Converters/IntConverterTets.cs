using NetDaemon.Fluent.Entities.Converters;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests.Converters
{
    public class IntConverterTets
    {
        private readonly ILogger _logger;

        private ITypeConverter _typeConverter;

        public IntConverterTets()
        {
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<IntConverter>().Returns(_logger);

            _typeConverter = new IntConverter(_logger);
        }

        [Fact]
        public void CanConvertFromDataPoint_ValidString_ReturnsInt()
        {
            var result = _typeConverter.FromDataPoint("123");

            Assert.Equal(123, result);
        }

        [Fact]
        public void CanConvertFromDataPoint_InvalidString_ReturnsDefault()
        {
            var result = _typeConverter.FromDataPoint("abc");

            _logger.Received().Error(Arg.Any<string>());

            Assert.Equal(0, result);
        }

        [Fact]
        public void ToSensorString_ValidInt_ReturnsString()
        {
            var result = _typeConverter.ToSensorString(123);

            Assert.Equal("123", result);
        }

        [Fact]
        public void ToSensorString_InvalidType_ReturnsNull()
        {
            var result = _typeConverter.ToSensorString("abc");
            _logger.Received().Error(Arg.Any<string>());
            Assert.Null(result);
        }

        [Fact]
        public void ToSensorString_NullValue_ReturnsNull()
        {
            var result = _typeConverter.ToSensorString(null);
            _logger.Received().Error(Arg.Any<string>());
            Assert.Null(result);
        }
    }
}
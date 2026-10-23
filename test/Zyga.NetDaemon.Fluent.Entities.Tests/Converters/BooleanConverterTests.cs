using NetDaemon.Fluent.Entities.Converters;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests.Converters
{
    public class BooleanConverterTests
    {
        private readonly ILogger _logger;

        private readonly ITypeConverter _typeConverter;

        public BooleanConverterTests()
        {
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<BooleanConverter>().Returns(_logger);

            _typeConverter = new BooleanConverter(_logger);
        }

        [Fact]
        public void CanConvertFromDataPoint_ValidBoolString_ReturnsBool()
        {
            var result = _typeConverter.FromDataPoint("true");

            Assert.Equal(true, result);
        }

        [Fact]
        public void CanConvertFromDataPoint_FalseBoolString_ReturnsFalse()
        {
            var result = _typeConverter.FromDataPoint("false");

            Assert.Equal(false, result);
        }

        [Fact]
        public void CanConvertFromDataPoint_OnSensorString_FallsBackAndReturnsTrue()
        {
            // "ON" is not parseable by bool.TryParse, so FromDataPoint falls back to the sensor-string convention.
            var result = _typeConverter.FromDataPoint(BooleanConverter.TrueValue);

            Assert.Equal(true, result);
        }

        [Fact]
        public void CanConvertFromDataPoint_OffSensorString_FallsBackAndReturnsFalse()
        {
            var result = _typeConverter.FromDataPoint(BooleanConverter.FalseValue);

            Assert.Equal(false, result);
        }

        [Fact]
        public void CanConvertFromDataPoint_InvalidString_ReturnsDefault()
        {
            var result = _typeConverter.FromDataPoint("garbage");

            _logger.Received().Error(Arg.Any<string>());

            Assert.Equal(false, result);
        }

        [Fact]
        public void FromSensorString_NullValue_ReturnsNull()
        {
            var result = _typeConverter.FromSensorString(null);

            Assert.Null(result);
        }

        [Fact]
        public void FromSensorString_TrueValue_ReturnsTrue()
        {
            var result = _typeConverter.FromSensorString(BooleanConverter.TrueValue);

            Assert.Equal(true, result);
        }

        [Fact]
        public void FromSensorString_FalseValue_ReturnsFalse()
        {
            var result = _typeConverter.FromSensorString(BooleanConverter.FalseValue);

            Assert.Equal(false, result);
        }

        [Fact]
        public void FromSensorString_IsCaseInsensitive()
        {
            var result = _typeConverter.FromSensorString(BooleanConverter.TrueValue.ToLowerInvariant());

            Assert.Equal(true, result);
        }

        [Fact]
        public void FromSensorString_UnrecognizedValue_ReturnsNull()
        {
            var result = _typeConverter.FromSensorString("garbage");

            Assert.Null(result);
        }

        [Fact]
        public void ToSensorString_NullValue_ReturnsUnknownValue()
        {
            var result = _typeConverter.ToSensorString(null);

            Assert.Equal(BooleanConverter.UnknownValue, result);
        }

        [Fact]
        public void ToSensorString_True_ReturnsTrueValue()
        {
            var result = _typeConverter.ToSensorString(true);

            Assert.Equal(BooleanConverter.TrueValue, result);
        }

        [Fact]
        public void ToSensorString_False_ReturnsFalseValue()
        {
            var result = _typeConverter.ToSensorString(false);

            Assert.Equal(BooleanConverter.FalseValue, result);
        }

        [Fact]
        public void ToSensorString_InvalidType_ReturnsNull()
        {
            var result = _typeConverter.ToSensorString("abc");

            _logger.Received().Error(Arg.Any<string>());

            Assert.Null(result);
        }
    }
}

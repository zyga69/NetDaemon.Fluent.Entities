using NetDaemon.Fluent.Entities.Converters;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests.Converters
{
    public class DateTimeConverterTests
    {
        private readonly ILogger _logger;
        private ITypeConverter _typeConverter;

        public DateTimeConverterTests()
        {
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<DateTimeConverter>().Returns(_logger);

            _typeConverter = new DateTimeConverter(_logger);
        }

        [Fact]
        public void CanConvertFromDataPoint_NoTimeZone_ReturnsDateTime()
        {
            DateTime dateTime = new DateTime(2025,4,15,12,30,45);

            string dateTimeString = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            var result = _typeConverter.FromDataPoint(dateTimeString);

            Assert.Equal(typeof(DateTime), result.GetType());

            var castedResult = (DateTime)result;

            Assert.Equal(DateTimeKind.Local, castedResult.Kind);

            Assert.Equal(dateTime, castedResult);
        }



        [Fact]
        public void CanConvertFromDataPoint_InvalidString_ReturnsDefault()
        {
            var result = _typeConverter.FromDataPoint("invalid-date");

            _logger.Received().Error(Arg.Any<string>());

            Assert.Equal(default(DateTime), result);
        }

        [Fact]
        public void ToSensorString_ValidDateTime_ReturnsString()
        {
            var dateTime = DateTime.Parse("2023-10-01T12:34:56Z").ToLocalTime();
            var result = _typeConverter.ToSensorString(dateTime);

            Assert.Equal(dateTime.ToString("o"), result);
        }

        [Fact]
        public void ToSensorString_InvalidType_ReturnsNull()
        {
            var result = _typeConverter.ToSensorString("invalid-type");

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
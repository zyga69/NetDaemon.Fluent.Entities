using System.Globalization;
using NetDaemon.Fluent.Entities.Converters;
using NSubstitute;
using Serilog;

namespace NetDaemon.Fluent.Entities.Tests.Converters
{
    public class DoubleConverterTests
    {
        private readonly ILogger _logger;

        private readonly ITypeConverter _typeConverter;

        public DoubleConverterTests()
        {
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<DoubleConverter>().Returns(_logger);

            _typeConverter = new DoubleConverter(_logger);
        }

        /// <summary>Runs <paramref name="action"/> with the current thread's culture temporarily switched, restoring it afterwards.</summary>
        private static void WithCulture(string cultureName, Action action)
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
                action();
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [Fact]
        public void CanConvertFromDataPoint_ValidString_ReturnsDouble()
        {
            WithCulture("en-US", () =>
            {
                var result = _typeConverter.FromDataPoint("123.45");

                Assert.Equal(123.45, result);
            });
        }

        [Fact]
        public void CanConvertFromDataPoint_InvalidString_ReturnsDefault()
        {
            var result = _typeConverter.FromDataPoint("abc");

            _logger.Received().Error(Arg.Any<string>());

            Assert.Equal(0d, result);
        }

        [Fact]
        public void CanConvertFromDataPoint_IsCultureSensitive_CharacterizesCurrentBehavior()
        {
            // CONCERN (not fixed here, out of scope): DoubleConverter.FromDataPoint calls
            // double.TryParse(value, out result) without specifying CultureInfo.InvariantCulture, so it
            // relies on the ambient thread culture. Under a culture that doesn't use '.' as its decimal
            // separator (e.g. fr-FR, which uses ','), a dot-decimal string like "123.45" fails to parse and
            // this falls back to the same default/error path as a genuinely invalid string. This test pins
            // that CURRENT, locale-dependent behavior - it does not endorse it.
            WithCulture("fr-FR", () =>
            {
                var result = _typeConverter.FromDataPoint("123.45");

                _logger.Received().Error(Arg.Any<string>());

                Assert.Equal(0d, result);
            });
        }

        [Fact]
        public void ToSensorString_ValidDouble_ReturnsString()
        {
            WithCulture("en-US", () =>
            {
                var result = _typeConverter.ToSensorString(123.45);

                Assert.Equal("123.45", result);
            });
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
            // Unlike BooleanConverter, DoubleConverter.ToSensorString has no explicit null case: null fails
            // the `value is double` pattern match and falls through to the same error branch as an
            // unrelated type, logging an error and returning null (rather than a dedicated "unknown" value).
            var result = _typeConverter.ToSensorString(null);

            _logger.Received().Error(Arg.Any<string>());

            Assert.Null(result);
        }

        [Fact]
        public void FromSensorString_ValidString_ReturnsDouble()
        {
            WithCulture("en-US", () =>
            {
                var result = _typeConverter.FromSensorString("123.45");

                Assert.Equal(123.45, result);
            });
        }

        [Fact]
        public void FromSensorString_NullValue_ReturnsNull()
        {
            var result = _typeConverter.FromSensorString(null);

            Assert.Null(result);
        }
    }
}

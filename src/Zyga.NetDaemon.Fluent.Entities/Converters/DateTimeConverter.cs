using Serilog;

namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>
    /// Converts <see cref="DateTime"/> device model values to/from the ISO 8601 ("o" format) string HA expects
    /// for <c>timestamp</c>-class sensors. Unspecified-kind values are treated as local time.
    /// </summary>
    internal class DateTimeConverter : SimpleConverter
    {
        private readonly ILogger _logger;

        /// <summary>Creates the converter.</summary>
        /// <param name="logger">The Serilog logger.</param>
        public DateTimeConverter(ILogger logger)
        {
            _logger = logger.ForContext<DateTimeConverter>();
        }

        /// <summary>
        /// Parses a raw string into a <see cref="DateTime"/>, treating an unspecified kind as local time.
        /// Returns <c>default(DateTime)</c> if parsing fails.
        /// </summary>
        /// <param name="value">The raw value to convert.</param>
        /// <returns>The converted <see cref="DateTime"/>, boxed as <see cref="object"/>.</returns>
        public override object FromDataPoint(string value)
        {
            if (DateTime.TryParse(value, out DateTime result))
            {
                if (result.Kind == DateTimeKind.Unspecified)
                    result = DateTime.SpecifyKind(result, DateTimeKind.Local);

                return result;
            }

            _logger.Error($"Failed to convert '{value}' to DateTime");

            return default(DateTime);
        }

        /// <summary>
        /// Converts a boxed <see cref="DateTime"/> (treating an unspecified kind as local time) to its ISO 8601
        /// ("o" format) string representation.
        /// </summary>
        /// <param name="value">The boxed <see cref="DateTime"/> value.</param>
        /// <returns>The ISO 8601 string, or <c>null</c> when <paramref name="value"/> is not a <see cref="DateTime"/>.</returns>
        public override string? ToSensorString(object? value)
        {
            if (value is DateTime dateTimeValue)
            {
                if (dateTimeValue.Kind == DateTimeKind.Unspecified)
                    dateTimeValue = DateTime.SpecifyKind(dateTimeValue, DateTimeKind.Local);

                return dateTimeValue.ToString("o");
            }

            _logger.Error($"Passed {value} is not an DateTime");

            return null;
        }
    }
}
using Serilog;

namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>Converts <see cref="double"/> device model values to/from their plain string representation.</summary>
    internal class DoubleConverter : SimpleConverter
    {
        private readonly ILogger _logger;

        /// <summary>Creates the converter.</summary>
        /// <param name="logger">The Serilog logger.</param>
        public DoubleConverter(ILogger logger)
        {
            _logger = logger.ForContext<DoubleConverter>();
        }

        /// <summary>Parses a raw string into a <see cref="double"/>. Returns <c>default(double)</c> (<c>0</c>) if parsing fails.</summary>
        /// <param name="value">The raw value to convert.</param>
        /// <returns>The converted <see cref="double"/>, boxed as <see cref="object"/>.</returns>
        public override object FromDataPoint(string value)
        {
            if (double.TryParse(value, out double result))
                return result;

            _logger.Error($"Failed to convert '{value}' to double");

            return default(double);
        }

        /// <summary>Converts a boxed <see cref="double"/> to its plain string representation.</summary>
        /// <param name="value">The boxed <see cref="double"/> value.</param>
        /// <returns>The string representation, or <c>null</c> when <paramref name="value"/> is not a <see cref="double"/>.</returns>
        public override string? ToSensorString(object? value)
        {
            if (value is double inValue)
                return inValue.ToString();

            _logger.Error($"Passed {value} is not an double");
            
            return null;
        }
    }
}
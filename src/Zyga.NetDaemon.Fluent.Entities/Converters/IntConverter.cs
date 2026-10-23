using Serilog;

namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>Converts <see cref="int"/> device model values to/from their plain string representation.</summary>
    internal class IntConverter : SimpleConverter
    {
        private readonly ILogger _logger;

        /// <summary>Creates the converter.</summary>
        /// <param name="logger">The Serilog logger.</param>
        public IntConverter(ILogger logger)
        {
            _logger = logger.ForContext<IntConverter>();
        }

        /// <summary>Parses a raw string into an <see cref="int"/>. Returns <c>default(int)</c> (<c>0</c>) if parsing fails.</summary>
        /// <param name="value">The raw value to convert.</param>
        /// <returns>The converted <see cref="int"/>, boxed as <see cref="object"/>.</returns>
        public override object FromDataPoint(string value)
        {
            if (int.TryParse(value, out int result))
                return result;

            _logger.Error($"Failed to convert '{value}' to int");

            return default(int);
        }

        /// <summary>Converts a boxed <see cref="int"/> to its plain string representation.</summary>
        /// <param name="value">The boxed <see cref="int"/> value.</param>
        /// <returns>The string representation, or <c>null</c> when <paramref name="value"/> is not an <see cref="int"/>.</returns>
        public override string? ToSensorString(object? value)
        {
            if (value is int inValue)
                return inValue.ToString();

            _logger.Error($"Passed {value} is not an int");

            return null;
        }
    }
}
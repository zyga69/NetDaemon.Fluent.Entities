using Serilog;

namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>
    /// Converts <see cref="bool"/> device model values to/from the HA MQTT sensor string representation
    /// (<c>ON</c>/<c>OFF</c>), and from raw external data point strings.
    /// </summary>
    internal class BooleanConverter : ITypeConverter
    {
        /// <summary>The MQTT sensor string representing <c>true</c>.</summary>
        public static readonly string TrueValue = "ON";

        /// <summary>The MQTT sensor string representing <c>false</c>.</summary>
        public static readonly string FalseValue = "OFF";

        /// <summary>The MQTT sensor string published when the value is unknown.</summary>
        public static readonly string UnknownValue = "unavailable";

        private readonly ILogger _logger;

        /// <summary>Creates the converter.</summary>
        /// <param name="logger">The Serilog logger.</param>
        public BooleanConverter(ILogger logger)
        {
            _logger = logger.ForContext<BooleanConverter>();
        }

        /// <summary>
        /// Converts a raw external data point string to a <see cref="bool"/>. Tries <see cref="bool.TryParse(string, out bool)"/>
        /// first, then falls back to the <see cref="TrueValue"/>/<see cref="FalseValue"/> sensor string convention.
        /// Returns <c>default(bool)</c> (<c>false</c>) if neither succeeds.
        /// </summary>
        /// <param name="value">The raw value to convert.</param>
        /// <returns>The converted <see cref="bool"/>, boxed as <see cref="object"/>.</returns>
        public object FromDataPoint(string value)
        {
            _logger.Verbose($"{nameof(FromDataPoint)} Converting [{value}] to bool");

            if (bool.TryParse(value, out bool result))
            {
                _logger.Verbose($"{nameof(FromDataPoint)} Successfully converted [{value}] to bool [{result}]");
                return result;
            }
                

            var sensorStringValue = FromSensorString(value);
            if (sensorStringValue == null)
            {
                _logger.Error($"{nameof(FromDataPoint)} Failed to convert [{value}] to bool");
                return default(bool);
            }
            else
            {
                return sensorStringValue;
            }
        }

        /// <summary>
        /// Parses an MQTT sensor string (<see cref="TrueValue"/>/<see cref="FalseValue"/>, case-insensitive) back
        /// into a <see cref="bool"/>.
        /// </summary>
        /// <param name="value">The sensor string to parse, or <c>null</c>.</param>
        /// <returns>The parsed <see cref="bool"/>, or <c>null</c> when <paramref name="value"/> is <c>null</c> or not recognized.</returns>
        public object? FromSensorString(string? value)
        {
            _logger.Verbose($"{nameof(FromSensorString)} Converting [{value}] to bool");

            if (value == null)
                return null;

            if (value.Equals(TrueValue, StringComparison.CurrentCultureIgnoreCase))
                return true;

            if (value.Equals(FalseValue, StringComparison.CurrentCultureIgnoreCase))
                return false;

            return null;
        }

        /// <summary>
        /// Converts a boxed <see cref="bool"/> to its MQTT sensor string (<see cref="TrueValue"/>/<see cref="FalseValue"/>),
        /// or <see cref="UnknownValue"/> when <paramref name="value"/> is <c>null</c>.
        /// </summary>
        /// <param name="value">The boxed <see cref="bool"/> value, or <c>null</c>.</param>
        /// <returns>The sensor string, or <c>null</c> when <paramref name="value"/> is neither <c>null</c> nor a <see cref="bool"/>.</returns>
        public string? ToSensorString(object? value)
        {
            if (value == null)
                return UnknownValue;

            if (value is bool inValue)
                return inValue ? TrueValue : FalseValue;

            _logger.Error($"{nameof(ToSensorString)} Passed [{value}] is not an bool");

            return null;
        }
    }
}
namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>
    /// Converts a device model value of a particular CLR type to/from the string representations used by
    /// MQTT sensor state and by external raw data points. Implementations are resolved by type through
    /// <see cref="IConverterFactory"/>.
    /// </summary>
    internal interface ITypeConverter
    {
        /// <summary>Converts a raw external data point string into the CLR value it represents.</summary>
        /// <param name="value">The raw value to convert.</param>
        /// <returns>The converted value, boxed as <see cref="object"/>.</returns>
        object FromDataPoint(string value);

        /// <summary>Converts a boxed device model value into the string published as MQTT sensor state.</summary>
        /// <param name="value">The boxed value to convert, or <c>null</c>.</param>
        /// <returns>The sensor string, or <c>null</c> when the value cannot be converted.</returns>
        string? ToSensorString(object? value);

        /// <summary>Parses an MQTT sensor state string back into the CLR value it represents.</summary>
        /// <param name="value">The sensor string to parse, or <c>null</c>.</param>
        /// <returns>The parsed value, boxed as <see cref="object"/>, or <c>null</c>.</returns>
        object? FromSensorString(string? value);
    }
}
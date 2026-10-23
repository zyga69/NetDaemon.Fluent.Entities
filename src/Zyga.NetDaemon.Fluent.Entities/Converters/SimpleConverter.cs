namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>
    /// Base <see cref="ITypeConverter"/> implementation whose default behavior treats sensor strings and raw
    /// data point strings identically (no numeric/date parsing). Subclasses override
    /// <see cref="FromDataPoint"/>/<see cref="ToSensorString"/> for types that need real conversion; <see cref="StringConverter"/>
    /// uses the defaults as-is.
    /// </summary>
    internal abstract class SimpleConverter : ITypeConverter
    {
        /// <summary>Returns <paramref name="value"/> unchanged.</summary>
        /// <param name="value">The raw value to convert.</param>
        /// <returns><paramref name="value"/>, boxed as <see cref="object"/>.</returns>
        public virtual object FromDataPoint(string value) => value;

        /// <summary>Delegates to <see cref="FromDataPoint"/>, or returns <c>null</c> when <paramref name="value"/> is <c>null</c>.</summary>
        /// <param name="value">The sensor string to parse, or <c>null</c>.</param>
        /// <returns>The converted value, or <c>null</c>.</returns>
        public virtual object? FromSensorString(string? value)
        {
            if (value == null) return null;

            return FromDataPoint(value);
        }

        /// <summary>Casts <paramref name="value"/> to <see cref="string"/>.</summary>
        /// <param name="value">The boxed value, expected to already be a <see cref="string"/> or <c>null</c>.</param>
        /// <returns><paramref name="value"/> cast to <see cref="string"/>.</returns>
        public virtual string? ToSensorString(object? value) => (string?)value;
    }
}
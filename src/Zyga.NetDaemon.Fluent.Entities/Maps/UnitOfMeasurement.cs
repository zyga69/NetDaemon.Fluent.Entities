using System.ComponentModel;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <summary>The Home Assistant unit-of-measurement values this library can assign to a sensor entity.</summary>
    public enum UnitOfMeasurement
    {
        /// <summary>Degrees Celsius, used by <c>AsTemperatureSensor()</c>.</summary>
        [Description("°C")]
        Celsius,
    }
}
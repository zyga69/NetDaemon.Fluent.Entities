using System.ComponentModel;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <summary>The Home Assistant <c>device_class</c> values this library can assign to a sensor entity.</summary>
    public enum DeviceClass
    {
        /// <summary>HA's <c>temperature</c> device class, used by <c>AsTemperatureSensor()</c>.</summary>
        [Description("temperature")]
        Temperature,

        /// <summary>HA's <c>timestamp</c> device class, used by <c>AsTimestamp()</c>.</summary>
        [Description("timestamp")]
        Timestamp,
    }
}
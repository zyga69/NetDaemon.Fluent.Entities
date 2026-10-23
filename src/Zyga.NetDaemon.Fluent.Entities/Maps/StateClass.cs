using System.ComponentModel;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <summary>The Home Assistant <c>state_class</c> values this library can assign to a sensor entity.</summary>
    public enum StateClass
    {
        /// <summary>HA's <c>measurement</c> state class: the sensor reports a point-in-time value.</summary>
        [Description("measurement")]
        Measurment
    }
}
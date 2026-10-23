using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Template
{
    /// <summary>
    /// Describes a HA template sensor whose value is (re)computed by invoking <see cref="Method"/> on the
    /// device model, pushed reactively whenever one of <see cref="TriggeringEntities"/> changes state (see
    /// <c>TemplateSensorService</c>). This is the push-based counterpart to <see cref="Sensor.SensorDefinition"/>.
    /// </summary>
    /// <param name="shortEntityId">The short (unprefixed) entity id, or a fully qualified one matching <paramref name="prefix"/>.</param>
    /// <param name="prefix">The HA domain prefix, e.g. <c>sensor</c> or <c>binary_sensor</c>.</param>
    /// <param name="deviceId">The owning device's identifier.</param>
    /// <param name="method">The device model method invoked to (re)compute the sensor's value.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public class TemplateSensorDefinition(string shortEntityId, string prefix, string deviceId, MethodInfo method, EntityAttributes? attributes = null)
        : EntityDefinition(shortEntityId, prefix, deviceId, attributes)
    {
        /// <summary>
        /// Gets the HA entities whose state changes trigger recomputation of this sensor's value (added via
        /// <c>TriggeredBy</c>/<c>Triggered.By</c>).
        /// </summary>
        public IList<NetDaemon.HassModel.Entities.Entity> TriggeringEntities { get; private set; } = [];


        /// <summary>
        /// Gets or sets the debounce duration applied before a triggered recomputation is acted on (set via
        /// <c>StableForSeconds</c>). When set, a new trigger resets the debounce timer, so only the state
        /// that remains stable for the full duration is used.
        /// </summary>
        public TimeSpan? StableTimeSpan { get; set; }

        /// <summary>Gets the device model method invoked to (re)compute the sensor's value.</summary>
        public MethodInfo Method { get; private set; } = method;

        /// <summary>Returns a diagnostic string representation of this definition.</summary>
        public override string ToString()
        {
            return $"[TemplateSensor|{Method.Name}]: {base.ToString()}";
        }
    }
}
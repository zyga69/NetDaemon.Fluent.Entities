using NetDaemon.Fluent.Entities.Model.EntityCreation;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Sensor
{
    /// <summary>
    /// Describes a HA <c>sensor</c>/<c>binary_sensor</c> entity backed by a plain (non-template) property on the
    /// device model. Its value is pushed by <c>SensorService</c> whenever <c>IDeviceService.Update</c> is called,
    /// rather than reactively on a triggering entity's state change (compare <see cref="Template.TemplateSensorDefinition"/>).
    /// </summary>
    /// <param name="shortEntityId">The short (unprefixed) entity id, or a fully qualified one matching <paramref name="prefix"/>.</param>
    /// <param name="prefix">The HA domain prefix (e.g. <c>sensor</c>, <c>binary_sensor</c>).</param>
    /// <param name="deviceId">The owning device's identifier.</param>
    /// <param name="property">The device model property backing the sensor's value.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public class SensorDefinition(string shortEntityId, string prefix, string deviceId, PropertyInfo property, EntityAttributes? attributes = null)
        : EntityDefinition(shortEntityId, prefix, deviceId, attributes)
    {
        /// <summary>Gets the device model property backing the sensor's value.</summary>
        public PropertyInfo Property { get; private set; } = property;

        /// <summary>Gets or sets the unit of measurement reported for this sensor, if any.</summary>
        [AdditionalConfig("unit_of_measurement")]
        public string? UnitOfMeasurement { get; set; }

        /// <summary>Gets or sets the HA state class (e.g. <c>measurement</c>) reported for this sensor.</summary>
        [AdditionalConfig("state_class")]
        public string? StateClass { get; set; }

        /// <summary>
        /// Gets or sets an external numeric identifier correlating this sensor with a data point from an
        /// external source (set via <c>WithId(int)</c>). Used by <c>IDeviceService.GetSensorIds</c>/<c>Update</c>
        /// to match incoming <see cref="Model.IIdValue"/> data points to the right sensor.
        /// </summary>
        public int? SensorId { get; set; }

        /// <summary>Returns a diagnostic string representation of this definition.</summary>
        public override string ToString()
        {
            return $"[Sensor|{Property.Name}] SensorId:[{SensorId}] UoM:[{UnitOfMeasurement}] {base.ToString()}";
        }
    }
}
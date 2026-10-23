using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Sensor
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsSensor()</c>. Registers the mapped property as a plain HA
    /// <c>sensor</c> entity, with no particular device class.
    /// </summary>
    /// <param name="propertyInfo">The device model property backing the sensor's value.</param>
    /// <param name="entityId">The HA entity ID (short or fully qualified with the <c>sensor</c> prefix).</param>
    /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public class GenericSensorMapRoot(PropertyInfo propertyInfo, string entityId, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
        : SensorMapRoot<GenericSensorMapRoot>(propertyInfo, entityId, "sensor", deviceDefinition, attributes)
    {
    }
}
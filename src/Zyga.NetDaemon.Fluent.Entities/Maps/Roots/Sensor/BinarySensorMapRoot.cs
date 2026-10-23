using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Sensor
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsBinarySensor()</c>. Registers the mapped property as a HA
    /// <c>binary_sensor</c> entity.
    /// </summary>
    /// <param name="propertyInfo">The device model property backing the sensor's value.</param>
    /// <param name="entityId">The HA entity ID (short or fully qualified with the <c>binary_sensor</c> prefix).</param>
    /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public class BinarySensorMapRoot(PropertyInfo propertyInfo, string entityId, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
       : SensorMapRoot<BinarySensorMapRoot>(propertyInfo, entityId, "binary_sensor", deviceDefinition, attributes)
    {
    }
}
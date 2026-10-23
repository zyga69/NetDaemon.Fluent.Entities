using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Sensor
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsTimestamp()</c>. Registers the mapped property as a HA
    /// <c>sensor</c> entity with the <c>timestamp</c> device class.
    /// </summary>
    public class TimeStampSensorMapRoot : SensorMapRoot<TimeStampSensorMapRoot>
    {
        /// <summary>Creates and registers the timestamp sensor's <see cref="SensorDefinition"/>.</summary>
        /// <param name="propertyInfo">The device model property backing the sensor's value.</param>
        /// <param name="entityId">The HA entity ID (short or fully qualified with the <c>sensor</c> prefix).</param>
        /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public TimeStampSensorMapRoot(PropertyInfo propertyInfo, string entityId, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
            : base(propertyInfo, entityId, "sensor", deviceDefinition, attributes)
        {
            _sensorDefinition.DeviceClass = DeviceClass.Timestamp.GetDescription();
        }
    }
}
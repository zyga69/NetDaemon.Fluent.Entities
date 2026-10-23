using Material.Icons;
using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Sensor
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsTemperatureSensor()</c>. Registers the mapped property as a HA
    /// <c>sensor</c> entity with the <c>temperature</c> device class, Celsius unit of measurement, and a
    /// thermometer icon applied automatically.
    /// </summary>
    public class TemperatureSensorMapRoot : SensorMapRoot<TemperatureSensorMapRoot>
    {
        /// <summary>Creates and registers the temperature sensor's <see cref="SensorDefinition"/>.</summary>
        /// <param name="propertyInfo">The device model property backing the sensor's value.</param>
        /// <param name="entityId">The HA entity ID (short or fully qualified with the <c>sensor</c> prefix).</param>
        /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public TemperatureSensorMapRoot(PropertyInfo propertyInfo, string entityId, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
            : base(propertyInfo, entityId, "sensor", deviceDefinition, attributes)
        {
            _sensorDefinition.UnitOfMeasurement = UnitOfMeasurement.Celsius.GetDescription();
            _sensorDefinition.DeviceClass = DeviceClass.Temperature.GetDescription();
            _sensorDefinition.Icon = MaterialIconKind.TemperatureCelsius.ToMdi();
        }
    }
}
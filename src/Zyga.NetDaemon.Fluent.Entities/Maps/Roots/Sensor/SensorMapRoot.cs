using Material.Icons;
using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.Model;
using System.Reflection;
using System.Text.RegularExpressions;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Sensor
{
    /// <summary>
    /// Base fluent root shared by the property-backed sensor terminals (<see cref="GenericSensorMapRoot"/>,
    /// <see cref="BinarySensorMapRoot"/>, <see cref="TemperatureSensorMapRoot"/>, <see cref="TimeStampSensorMapRoot"/>).
    /// Builds and registers the <see cref="SensorDefinition"/>, defaulting its state class to <c>measurement</c>.
    /// </summary>
    /// <typeparam name="TMap">The concrete root type, enabling a fluent API via the curiously recurring template pattern.</typeparam>
    public abstract class SensorMapRoot<TMap>
        where TMap : SensorMapRoot<TMap>
    {
        /// <summary>The sensor definition being built by this root and its subclasses.</summary>
        protected readonly SensorDefinition _sensorDefinition;

        /// <summary>Creates and registers the <see cref="SensorDefinition"/> for the mapped property.</summary>
        /// <param name="propertyInfo">The device model property backing the sensor's value.</param>
        /// <param name="entityId">The HA entity ID (short or fully qualified with <paramref name="prefix"/>).</param>
        /// <param name="prefix">The HA domain prefix (e.g. <c>sensor</c>, <c>binary_sensor</c>).</param>
        /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public SensorMapRoot(PropertyInfo propertyInfo, string entityId, string prefix, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
        {
            _sensorDefinition = new SensorDefinition(entityId, prefix, deviceDefinition.Identifier, propertyInfo, attributes)
            {
                StateClass = StateClass.Measurment.GetDescription()
            };

            deviceDefinition.Entities.Add(_sensorDefinition);
        }

        /// <summary>
        /// Associates this sensor with an external numeric data point identifier, so values pushed through
        /// <c>IDeviceService.Update</c>/<c>UpdateKeyed</c> with matching <see cref="Model.IIdValue.Id"/> are routed
        /// to this sensor's property.
        /// </summary>
        /// <param name="sensorId">The external data point identifier.</param>
        /// <returns>The current builder instance.</returns>
        public TMap WithId(int sensorId)
        {
            _sensorDefinition.SensorId = sensorId;
            return (TMap)this;
        }


    }
}
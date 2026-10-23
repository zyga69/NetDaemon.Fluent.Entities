using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Template
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsTemplateSensor()</c>. Builds the <see cref="TemplateSensorDefinition"/>
    /// for the mapped method and lets the caller wire up one or more triggering entities (<see cref="TriggeredBy"/>
    /// or <see cref="Triggered"/>.<c>By(...)</c>) and an optional debounce (<see cref="StableForSeconds"/>).
    /// The definition is only added to the device once at least one triggering entity has been registered -
    /// a template sensor with no trigger would never update.
    /// </summary>
    public class TemplateSensorMapRoot : IMultiTriggerSensor, ISingleTriggerSensor, IMayComplete
    {
        private readonly TemplateSensorDefinition _sensorDefinition;

        private readonly DeviceDefinition _deviceDefinition;

        /// <summary>Creates the template sensor's definition using an explicit HA domain prefix.</summary>
        /// <param name="method">The device model method invoked to (re)compute the sensor's value.</param>
        /// <param name="entityId">The HA entity ID (short or fully qualified with <paramref name="prefix"/>).</param>
        /// <param name="prefix">The HA domain prefix, e.g. <c>sensor</c> or <c>binary_sensor</c>.</param>
        /// <param name="deviceDefinition">The owning device definition the entity will be registered with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public TemplateSensorMapRoot(MethodInfo method, string entityId, string prefix, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
        {
            _sensorDefinition = new(entityId, prefix, deviceDefinition.Identifier, method, attributes);
            _deviceDefinition = deviceDefinition;
        }

        /// <summary>
        /// Creates the template sensor's definition, inferring the HA domain prefix from the method's
        /// result type: <c>binary_sensor</c> for <see cref="bool"/> (or nullable <see cref="bool"/>), otherwise <c>sensor</c>.
        /// </summary>
        /// <param name="method">The device model method invoked to (re)compute the sensor's value.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <param name="resultType">The method's return type, used to infer the HA domain prefix.</param>
        /// <param name="deviceDefinition">The owning device definition the entity will be registered with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public TemplateSensorMapRoot(MethodInfo method, string entityId, Type resultType, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
        {
            Type underlyingType = Nullable.GetUnderlyingType(resultType) ?? resultType;
            string prefix = underlyingType == typeof(bool) ? "binary_sensor" : "sensor";

            _sensorDefinition = new(entityId, prefix, deviceDefinition.Identifier, method, attributes);
            _deviceDefinition = deviceDefinition;
        }

        /// <summary>
        /// Debounces recomputation: after a triggering entity changes state, the new value is only applied
        /// once its state has remained unchanged for <paramref name="seconds"/> seconds. A further change
        /// within that window restarts the wait.
        /// </summary>
        /// <param name="seconds">The number of seconds the triggering state must remain stable.</param>
        /// <returns>A builder restricted to adding a single triggering entity via <see cref="TriggeredBy"/>.</returns>
        public ISingleTriggerSensor StableForSeconds(int seconds)
        {
            _sensorDefinition.StableTimeSpan = TimeSpan.FromSeconds(seconds);
            return this;
        }

        /// <summary>Gets a builder for registering multiple triggering entities via chained <c>By(...)</c> calls.</summary>
        public IMultiTriggerSensor Triggered => this;

        /// <summary>Adds another triggering entity and returns the builder for further chaining.</summary>
        /// <param name="entity">The HA entity whose state changes trigger recomputation.</param>
        IMultiTriggerSensor IMultiTriggerSensor.By(NetDaemon.HassModel.Entities.Entity entity)
        {
            TriggeredBy(entity);
            return this;
        }

        /// <summary>
        /// Registers the HA entity whose state changes trigger recomputation of this sensor's value, and
        /// ensures the sensor's definition has been added to the owning device.
        /// </summary>
        /// <param name="entity">The HA entity whose state changes trigger recomputation.</param>
        /// <returns>A builder exposing <see cref="GetFullEntityId"/> to complete the mapping.</returns>
        public IMayComplete TriggeredBy(NetDaemon.HassModel.Entities.Entity entity)
        {
            _sensorDefinition.TriggeringEntities.Add(entity);

            if (!_deviceDefinition.Entities.Contains(_sensorDefinition))
                _deviceDefinition.Entities.Add(_sensorDefinition);

            return this;
        }

        /// <summary>
        /// Gets the fully composed entity id of this template sensor (e.g. <c>sensor.tank_nd_level_float</c>),
        /// so it can itself be used as the triggering entity of another mapped sensor.
        /// </summary>
        public string GetFullEntityId() => _sensorDefinition.EntityId;


    }

    /// <summary>Fluent builder for registering more than one triggering entity on a template sensor.</summary>
    public interface IMultiTriggerSensor : IMayComplete
    {
        /// <summary>Adds another triggering entity and returns the builder for further chaining.</summary>
        /// <param name="entity">The HA entity whose state changes trigger recomputation.</param>
        IMultiTriggerSensor By(NetDaemon.HassModel.Entities.Entity entity);
    }

    /// <summary>Fluent builder for registering exactly one triggering entity on a debounced template sensor.</summary>
    public interface ISingleTriggerSensor
    {
        /// <summary>Registers the triggering entity.</summary>
        /// <param name="entity">The HA entity whose state changes trigger recomputation.</param>
        IMayComplete TriggeredBy(NetDaemon.HassModel.Entities.Entity entity);
    }

    /// <summary>Marks a template sensor builder as having at least one triggering entity, so mapping can be completed.</summary>
    public interface IMayComplete
    {
        /// <summary>Gets the fully composed entity id of the template sensor.</summary>
        string GetFullEntityId();
    }
}
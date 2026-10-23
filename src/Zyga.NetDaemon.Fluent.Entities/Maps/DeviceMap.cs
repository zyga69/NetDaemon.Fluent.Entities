using Material.Icons;
using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;
using NetDaemon.Fluent.Entities.Model;
using System.Linq.Expressions;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <summary>
    /// Base class for mapping a Home Assistant device to a strongly-typed <typeparamref name="TDevice"/> model.
    /// Derive from this class to declare which HA entities correspond to properties and methods of the device.
    /// </summary>
    /// <typeparam name="TDevice">The device model type being mapped.</typeparam>
    public abstract class DeviceMap<TDevice> : IDeviceMap, IDeviceMapDefined
        where TDevice : class
    {
        private DeviceDefinition? _definition;

        /// <summary>
        /// Registers the device with the given Home Assistant device identifier.
        /// </summary>
        /// <param name="identifier">The HA device identifier (e.g. device registry entry ID).</param>
        /// <returns>A fluent builder for further device configuration.</returns>
        public IDeviceMapDefined AsDevice(string identifier)
        {
            ArgumentNullException.ThrowIfNull(identifier);

            _definition = new DeviceDefinition(identifier, typeof(TDevice));

            return this;
        }

        /// <summary>
        /// Registers the device with the given Home Assistant device identifier and a lookup key.
        /// Use this overload when multiple devices of the same type need to be distinguished by key.
        /// </summary>
        /// <param name="identifier">The HA device identifier.</param>
        /// <param name="key">An arbitrary key used to differentiate instances of <typeparamref name="TDevice"/>.</param>
        /// <returns>A fluent builder for further device configuration.</returns>
        public IDeviceMapDefined AsDevice(string identifier, object key)
        {
            ArgumentNullException.ThrowIfNull(identifier);
            ArgumentNullException.ThrowIfNull(key);

            _definition = new DeviceDefinition(identifier, typeof(TDevice))
            {
                Key = key
            };

            return this;
        }

        /// <summary>
        /// Sets an optional human-readable name for the device, overriding the default HA display name.
        /// </summary>
        /// <param name="friendlyName">The friendly name to assign to the device.</param>
        void IDeviceMapDefined.WithFriendlyName(string friendlyName)
        {
            ArgumentNullException.ThrowIfNull(_definition);

            _definition.FriendlyName = friendlyName;
        }

        /// <summary>Maps an <see cref="int"/> property of <typeparamref name="TDevice"/> to a HA entity.</summary>
        /// <param name="property">Expression selecting the <see cref="int"/> property.</param>
        /// <param name="entityId">The HA entity ID (e.g. <c>sensor.living_room_temperature</c>).</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public GenericPropertyRoot<TDevice, int> Map(Expression<Func<TDevice, int>> property, string entityId)
            => new(property, entityId, Definition);

        /// <summary>Maps a <see cref="string"/> property of <typeparamref name="TDevice"/> to a HA entity.</summary>
        /// <param name="property">Expression selecting the <see cref="string"/> property.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public GenericPropertyRoot<TDevice, string> Map(Expression<Func<TDevice, string>> property, string entityId)
            => new(property, entityId, Definition);

        /// <summary>Maps a <see cref="double"/> property of <typeparamref name="TDevice"/> to a HA entity.</summary>
        /// <param name="property">Expression selecting the <see cref="double"/> property.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public GenericPropertyRoot<TDevice, double> Map(Expression<Func<TDevice, double>> property, string entityId)
            => new(property, entityId, Definition);

        /// <summary>Maps a <see cref="DateTime"/> property of <typeparamref name="TDevice"/> to a HA entity.</summary>
        /// <param name="property">Expression selecting the <see cref="DateTime"/> property.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public DateTimePropertyRoot<TDevice> Map(Expression<Func<TDevice, DateTime>> property, string entityId)
            => new(property, entityId, Definition);

        /// <summary>Maps a <see cref="bool"/> property of <typeparamref name="TDevice"/> to a HA entity.</summary>
        /// <param name="property">Expression selecting the <see cref="bool"/> property.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public BooleanPropertyRoot<TDevice> Map(Expression<Func<TDevice, bool?>> property, string entityId)
            => new(property, entityId, Definition);

        /// <summary>
        /// Maps a single-parameter method of <typeparamref name="TDevice"/> that accepts and returns a <see cref="bool"/> to a HA entity.
        /// Useful for toggle-style template sensors or binary controls.
        /// </summary>
        /// <param name="expression">Expression selecting the method.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public SingleParameterMethodRoot<TDevice, bool?, bool?> Map(Expression<Func<TDevice, bool?, bool?>> expression, string entityId)
            => new(expression, entityId, Definition);

        /// <summary>
        /// Maps a parameterless action method of <typeparamref name="TDevice"/> to a HA entity.
        /// Typically used to expose a device action as a HA button.
        /// </summary>
        /// <param name="handler">Expression selecting the void method on the device.</param>
        /// <param name="entityId">The HA entity ID.</param>
        /// <returns>A root builder for further mapping configuration.</returns>
        public ActionRoot Map(Expression<Action<TDevice>> handler, string entityId)
            => new(handler.GetMethodInfo(), entityId, Definition);

        /// <summary>
        /// Gets the <see cref="DeviceDefinition"/> built by the most recent <see cref="AsDevice(string)"/> call.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when <see cref="AsDevice(string)"/> has not yet been called.</exception>
        public DeviceDefinition Definition
        {
            get
            {
                if (_definition == null)
                    throw new InvalidOperationException("Device has not been properly defined");

                return _definition;
            }
        }
    }

    /// <summary>Marker interface for device maps registered as scoped services.</summary>
    public interface IDeviceMap : IScopedService
    {
        /// <summary>Gets the definition describing the mapped HA device.</summary>
        DeviceDefinition Definition { get; }
    }

    /// <summary>Fluent builder interface returned after a device identifier has been provided.</summary>
    public interface IDeviceMapDefined
    {
        /// <summary>Sets a human-readable name for the device.</summary>
        /// <param name="friendlyName">The friendly name to assign.</param>
        void WithFriendlyName(string friendlyName);
    }

    /// <summary>
    /// Base fluent builder providing common entity attribute configuration shared by all mapping roots.
    /// </summary>
    /// <typeparam name="TNext">The concrete root type, enabling a fluent API via the curiously recurring template pattern.</typeparam>
    public abstract class CommonRoot<TNext>
       where TNext : CommonRoot<TNext>
    {
        /// <summary>The entity attributes (friendly name, icon) accumulated by <see cref="WithFriendlyName"/>/<see cref="WithIcon"/> so far.</summary>
        protected EntityAttributes _attibutes = new();

        /// <summary>Sets the friendly name displayed for the entity in Home Assistant.</summary>
        /// <param name="name">The display name.</param>
        /// <returns>The current builder instance.</returns>
        public TNext WithFriendlyName(string name)
        {
            _attibutes.FriendlyName = name;
            return (TNext)this;
        }

        /// <summary>Sets the Material Design icon associated with the entity in Home Assistant.</summary>
        /// <param name="icon">The icon to use.</param>
        /// <returns>The current builder instance.</returns>
        public TNext WithIcon(MaterialIconKind icon)
        {
            _attibutes.Icon = icon.ToMdi();
            return (TNext)this;
        }
    }

    /// <summary>Fluent root for mapping a <see cref="DateTime"/> property to a specific HA sensor type.</summary>
    /// <typeparam name="TDevice">The device model type.</typeparam>
    public class DateTimePropertyRoot<TDevice>(Expression<Func<TDevice, DateTime>> expression, string entityId, DeviceDefinition deviceDefinition)
        : CommonRoot<DateTimePropertyRoot<TDevice>>
    {
        /// <summary>Configures the entity as a timestamp sensor.</summary>
        public TimeStampSensorMapRoot AsTimestamp() => new(expression.GetProperty(), entityId, deviceDefinition, _attibutes);
    }

    /// <summary>Fluent root for mapping a generic property of type <typeparamref name="T"/> to a HA sensor.</summary>
    /// <typeparam name="TDevice">The device model type.</typeparam>
    /// <typeparam name="T">The property value type (<see cref="int"/>, <see cref="string"/>, or <see cref="double"/>).</typeparam>
    public class GenericPropertyRoot<TDevice, T>(Expression<Func<TDevice, T>> expression, string entityId, DeviceDefinition deviceDefinition)
        : CommonRoot<GenericPropertyRoot<TDevice, T>>
    {
        /// <summary>Configures the entity as a generic sensor.</summary>
        public GenericSensorMapRoot AsSensor() => new(expression.GetProperty(), entityId, deviceDefinition, _attibutes);

        /// <summary>Configures the entity as a temperature sensor with the appropriate device class.</summary>
        public TemperatureSensorMapRoot AsTemperatureSensor() => new(expression.GetProperty(), entityId, deviceDefinition, _attibutes);

        /// <summary>Configures the entity as a template sensor backed by a method expression.</summary>
        public TemplateSensorMapRoot AsTemplateSensor() => new(expression.GetMethodInfo(), entityId, "sensor", deviceDefinition, _attibutes);
    }

    /// <summary>Fluent root for mapping a <see cref="bool"/> property to a HA binary sensor or template sensor.</summary>
    /// <typeparam name="TDevice">The device model type.</typeparam>
    public class BooleanPropertyRoot<TDevice>(Expression<Func<TDevice, bool?>> expression, string entityId, DeviceDefinition deviceDefinition)
        : CommonRoot<BooleanPropertyRoot<TDevice>>
    {
        /// <summary>Configures the entity as a binary sensor (on/off state).</summary>
        public BinarySensorMapRoot AsBinarySensor() => new(expression.GetProperty(), entityId, deviceDefinition, _attibutes);

        /// <summary>Configures the entity as a switch.</summary>
        public SwitchMapRoot AsSwitch() => new(expression.GetProperty(), entityId, deviceDefinition, _attibutes);

        /// <summary>Configures the entity as a <c>binary_sensor</c> template sensor backed by a method expression.</summary>
        public TemplateSensorMapRoot AsTemplateSensor() => new(expression.GetMethodInfo(), entityId, "binary_sensor", deviceDefinition, _attibutes);
    }

    /// <summary>Fluent root for mapping a parameterless action method to a HA button entity.</summary>
    public class ActionRoot(MethodInfo method, string entityId, DeviceDefinition deviceDefinition)
         : CommonRoot<ActionRoot>
    {
        /// <summary>Configures the entity as a pressable HA button that invokes the mapped method.</summary>
        public ButtonMapRoot AsButton() => new(method, entityId, deviceDefinition, _attibutes);
    }

    /// <summary>
    /// Fluent root for mapping a single-parameter method of <typeparamref name="TDevice"/> to a HA template sensor.
    /// </summary>
    /// <typeparam name="TDevice">The device model type.</typeparam>
    /// <typeparam name="TArg">The method's input parameter type.</typeparam>
    /// <typeparam name="TResult">The method's return type, which determines the sensor's value type.</typeparam>
    public class SingleParameterMethodRoot<TDevice, TArg, TResult>(Expression<Func<TDevice, TArg, TResult>> expression, string entityId, DeviceDefinition deviceDefinition)
        : CommonRoot<SingleParameterMethodRoot<TDevice, TArg, TResult>>
    {
        /// <summary>Configures the entity as a template sensor whose value is produced by the mapped method.</summary>
        public ISingleTriggerSensor AsTemplateSensor() => new TemplateSensorMapRoot(expression.GetMethodInfo(), entityId, typeof(TResult), deviceDefinition, _attibutes);
    }
}
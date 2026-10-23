using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;

namespace NetDaemon.Fluent.Entities.Model
{
    /// <summary>
    /// Describes a single mapped HA device: its identifier, model type, optional lookup key (for keyed
    /// devices - see <c>AsDevice(identifier, key)</c>), and the list of entities built up by the owning
    /// <c>DeviceMap</c>'s <c>Map(...)</c> calls. Tracked in the <c>IDeviceRegistry</c> once initialized.
    /// </summary>
    public class DeviceDefinition
    {
        /// <summary>Creates a device definition for the given identifier and model type.</summary>
        /// <param name="identifier">The HA device identifier (e.g. device registry entry ID).</param>
        /// <param name="type">The device model's CLR type.</param>
        public DeviceDefinition(string identifier, Type type)
        {
            Identifier = identifier;
            Type = type;
            Entities = [];
        }

        /// <summary>Gets or sets the HA device identifier.</summary>
        public string Identifier { get; set; }

        /// <summary>Gets or sets the device's current lifecycle state. Starts as <see cref="DeviceState.Uninitialized"/>.</summary>
        public DeviceState State { get; set; } = DeviceState.Uninitialized;

        /// <summary>Gets or sets the device model's CLR type.</summary>
        public Type Type { get; set; }

        /// <summary>Gets or sets the optional lookup key distinguishing this instance among several devices of the same <see cref="Type"/>.</summary>
        public object? Key { get; set; }

        /// <summary>Gets or sets the device's friendly (display) name.</summary>
        public string? FriendlyName { get; set; }

        /// <summary>Gets the subset of <see cref="Entities"/> that are property-backed sensors.</summary>
        public IEnumerable<SensorDefinition> Sensors => Entities.OfType<SensorDefinition>();

        /// <summary>Gets the subset of <see cref="Entities"/> that are buttons (action methods).</summary>
        public IEnumerable<ActionDefinition> Buttons => Entities.OfType<ActionDefinition>();

        /// <summary>Gets the full list of entities mapped for this device.</summary>
        public IList<EntityDefinition> Entities { get; }

        /// <summary>Returns a diagnostic string representation of this definition.</summary>
        public override string ToString()
        {
            return $"[Device|{Type.Name}|{Key}|{State}]: [{Identifier}] Friendly Name:[{FriendlyName}]";
        }
    }
}
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Switch
{
    /// <summary>
    /// Describes a HA <c>switch</c> entity backed by a <see cref="bool"/> property on the device model.
    /// <c>SwitchService</c> subscribes to the entity's HA commands and writes the resulting state back
    /// through MQTT; it does not currently write the value back onto the device model property.
    /// </summary>
    /// <param name="shortEntityId">The short (unprefixed) entity id, or a fully qualified one matching <paramref name="prefix"/>.</param>
    /// <param name="prefix">The HA domain prefix, typically <c>switch</c>.</param>
    /// <param name="deviceId">The owning device's identifier.</param>
    /// <param name="property">The device model property associated with the switch.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public class SwitchDefinition(string shortEntityId, string prefix, string deviceId, PropertyInfo property, EntityAttributes? attributes = null)
        : EntityDefinition(shortEntityId, prefix, deviceId, attributes)
    {
        /// <summary>Gets the device model property associated with the switch.</summary>
        public PropertyInfo Property { get; private set; } = property;

        /// <summary>
        /// Gets or sets the state the switch is set to when it is first created (set via
        /// <c>InitiallyOn()</c>/<c>InitiallyOff()</c>). Defaults to <c>false</c> (off).
        /// </summary>
        public bool InitialState { get; set; } = false;

        /// <summary>Returns a diagnostic string representation of this definition.</summary>
        public override string ToString()
        {
            return $"[Switch|{Property.Name}]";
        }
    }
}
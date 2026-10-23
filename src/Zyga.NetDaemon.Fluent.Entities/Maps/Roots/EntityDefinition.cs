using NetDaemon.Fluent.Entities.Model.EntityCreation;

namespace NetDaemon.Fluent.Entities.Maps.Roots
{
    /// <summary>
    /// Base description of a single HA entity mapped from a device model member. Concrete subclasses
    /// (<c>SensorDefinition</c>, <c>SwitchDefinition</c>, <c>ActionDefinition</c>, <c>TemplateSensorDefinition</c>)
    /// add the data needed by their respective <c>IEntityInitializer</c>.
    /// </summary>
    /// <param name="shortEntityId">The short (unprefixed) entity id, or a fully qualified one matching <paramref name="prefix"/>.</param>
    /// <param name="prefix">The HA domain prefix (e.g. <c>sensor</c>, <c>switch</c>, <c>button</c>).</param>
    /// <param name="deviceId">The owning device's identifier, used to compose the final entity id.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public abstract class EntityDefinition(string shortEntityId, string prefix, string deviceId, EntityAttributes? attributes = null)
    {
        /// <summary>Gets or sets the fully composed HA entity id (e.g. <c>sensor.tank_nd_level</c>).</summary>
        public string EntityId { get; set; } = ParsedEntityId.Parse(shortEntityId, prefix, deviceId).ToString();

        /// <summary>Gets or sets the friendly (display) name of the entity.</summary>
        public string? FriendlyName { get; set; } = attributes?.FriendlyName;

        /// <summary>Gets or sets the HA device class reported for this entity, if any.</summary>
        public string? DeviceClass { get; set; }

        /// <summary>
        /// Gets or sets the Material Design Icons identifier (<c>mdi:...</c>) for the entity. Marked with
        /// <see cref="AdditionalConfigAttribute"/> so it is included in the MQTT discovery payload.
        /// </summary>
        [AdditionalConfig("icon")]
        public string? Icon { get; set; } = attributes?.Icon;

        /// <summary>Returns a diagnostic string representation of this definition.</summary>
        public override string ToString()
        {
            return $"[{EntityId}] Friendly Name:[{FriendlyName}] Class:[{DeviceClass}] ";
        }
    }
}
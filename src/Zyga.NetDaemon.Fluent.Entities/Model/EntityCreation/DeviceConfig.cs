namespace NetDaemon.Fluent.Entities.Model.EntityCreation
{
#pragma warning disable IDE1006 // Naming Styles

    /// <summary>
    /// Shape of the <c>device</c> block of an MQTT discovery payload, identifying the HA device an entity
    /// belongs to. Property names are lowercase to match the JSON payload MQTT discovery expects.
    /// </summary>
    internal class DeviceConfig
    {
        /// <summary>Builds the device block from a <see cref="DeviceDefinition"/>.</summary>
        /// <param name="deviceDefinition">The device the entity belongs to.</param>
        public DeviceConfig(DeviceDefinition deviceDefinition)
        {
            ArgumentNullException.ThrowIfNull(deviceDefinition);

            identifiers = [deviceDefinition.Identifier];
            name = deviceDefinition.FriendlyName ?? deviceDefinition.Identifier;
        }

        /// <summary>Gets or sets the device's identifiers, as expected by HA's device registry.</summary>
        public string[] identifiers { get; set; }

        /// <summary>Gets or sets the device's display name, falling back to its identifier when no friendly name was set.</summary>
        public string name { get; set; }
    }

#pragma warning restore IDE1006 // Naming Styles
}
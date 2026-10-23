using NetDaemon.Fluent.Entities.Maps.Roots;

namespace NetDaemon.Fluent.Entities.Model.EntityCreation
{
#pragma warning disable IDE1006 // Naming Styles

    /// <summary>
    /// Shape of the "additional config" payload passed to MQTT discovery alongside an entity's standard
    /// options, mirroring the <c>device</c> key built by <c>EntityDefinitionExtensions.BuildAdditionalConfig</c>.
    /// Property name is lowercase to match the JSON payload MQTT discovery expects.
    /// </summary>
    internal class AdditionalConfig()
    {
        /// <summary>Gets or sets the device metadata block of the discovery payload.</summary>
        public DeviceConfig? device { get; set; }
    }

#pragma warning restore IDE1006 // Naming Styles
}
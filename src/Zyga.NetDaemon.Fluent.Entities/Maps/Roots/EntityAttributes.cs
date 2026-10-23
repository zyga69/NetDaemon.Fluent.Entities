namespace NetDaemon.Fluent.Entities.Maps.Roots
{
    /// <summary>
    /// Carries the optional display attributes (set via <c>WithFriendlyName</c>/<c>WithIcon</c> in the
    /// fluent API) accumulated by a <c>CommonRoot</c> before the terminal <c>As*()</c> call creates the
    /// concrete <see cref="EntityDefinition"/>.
    /// </summary>
    public class EntityAttributes
    {
        /// <summary>Gets or sets the friendly (display) name of the entity.</summary>
        public string? FriendlyName { get; set; }

        /// <summary>Gets or sets the Material Design Icons identifier (<c>mdi:...</c>) for the entity.</summary>
        public string? Icon { get; set; }
    }
}
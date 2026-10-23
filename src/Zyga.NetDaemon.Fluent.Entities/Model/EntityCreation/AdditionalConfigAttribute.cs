namespace NetDaemon.Fluent.Entities.Model.EntityCreation
{
    /// <summary>
    /// Marks an <c>EntityDefinition</c> property for inclusion in the MQTT discovery "additional config"
    /// payload under the given key, when its value is non-null (see
    /// <c>EntityDefinitionExtensions.BuildAdditionalConfig</c>). Used by <c>EntityDefinition.Icon</c> and
    /// <c>SensorDefinition.UnitOfMeasurement</c>/<c>StateClass</c>.
    /// </summary>
    /// <param name="name">The payload key the property's value is written under.</param>
    [AttributeUsage(AttributeTargets.Property)]
    internal class AdditionalConfigAttribute(string name) : Attribute
    {
        /// <summary>Gets or sets the payload key the property's value is written under.</summary>
        public string Name { get; set; } = name;
    }
}
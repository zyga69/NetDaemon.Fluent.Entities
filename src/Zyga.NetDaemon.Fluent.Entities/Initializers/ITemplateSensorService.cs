using NetDaemon.Fluent.Entities.DependencyInjection;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// Initializes HA template sensor entities for <see cref="Maps.Roots.Template.TemplateSensorDefinition"/>
    /// mappings: subscribes to each triggering entity's state changes and pushes a recomputed value.
    /// </summary>
    internal interface ITemplateSensorService : IEntityInitializer,
        IDisposable,
        IScopedService
    {
    }
}
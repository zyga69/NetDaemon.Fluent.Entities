using NetDaemon.Fluent.Entities.DependencyInjection;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>Initializes HA <c>switch</c> entities for <see cref="Maps.Roots.Switch.SwitchDefinition"/> mappings.</summary>
    internal interface ISwitchService : IEntityInitializer,
        IDisposable,
        IScopedService
    {
    }
}
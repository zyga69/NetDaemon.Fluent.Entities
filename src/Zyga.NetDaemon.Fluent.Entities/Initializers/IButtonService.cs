using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>Initializes HA <c>button</c> entities for <see cref="Maps.Roots.Button.ActionDefinition"/> mappings.</summary>
    public interface IButtonService : IEntityInitializer,
        IDisposable,
        IScopedService
    {
    }
}
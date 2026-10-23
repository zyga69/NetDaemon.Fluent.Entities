using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// Creates the HA entity described by an <see cref="EntityDefinition"/> for a given device. Implementations
    /// are registered keyed by the concrete <see cref="EntityDefinition"/> subclass they handle (see
    /// <c>AddEntityInitializers</c>) and resolved through <see cref="IEntityInitializerFactory"/>.
    /// </summary>
    public interface IEntityInitializer
    {
        /// <summary>Creates (and, where applicable, subscribes to) the HA entity described by <paramref name="definition"/>.</summary>
        /// <param name="definition">The entity definition to initialize.</param>
        /// <param name="deviceDefinition">The owning device definition.</param>
        /// <returns>A task representing the asynchronous initialization.</returns>
        Task InitWithDevice(EntityDefinition definition, DeviceDefinition deviceDefinition);
    }
}
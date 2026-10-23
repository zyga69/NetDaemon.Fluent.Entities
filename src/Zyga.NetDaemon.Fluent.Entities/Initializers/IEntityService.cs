using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// Thin wrapper over <c>IMqttEntityManager</c> that creates HA entities via MQTT discovery and toggles
    /// their availability. Disposing the service disables every entity it created.
    /// </summary>
    public interface IEntityService : IAsyncDisposable, IScopedService
    {
        /// <summary>Publishes the MQTT discovery message creating the entity described by <paramref name="entityDefinition"/>.</summary>
        /// <param name="entityDefinition">The entity to create.</param>
        /// <param name="deviceDefinition">The owning device, used to populate the discovery payload's device metadata.</param>
        /// <returns>A task representing the asynchronous creation.</returns>
        Task Create(EntityDefinition entityDefinition, DeviceDefinition? deviceDefinition = null);

        /// <summary>Marks the entity as unavailable in HA.</summary>
        /// <param name="entityId">The entity's HA id.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Disable(string entityId);

        /// <summary>Marks the entity as available in HA.</summary>
        /// <param name="entityId">The entity's HA id.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task Enable(string entityId);
    }
}
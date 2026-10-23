using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Extensions;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Model.EntityCreation;

namespace NetDaemon.Fluent.Entities
{
    /// <inheritdoc/>
    internal class EntityService : IEntityService
    {
        /// <summary>The MQTT availability payload published when an entity is enabled.</summary>
        public const string AVAILABLE = "nd-ON";
        /// <summary>The MQTT availability payload published when an entity is disabled.</summary>
        public const string NOT_AVAILABLE = "nd-OFF";

        private readonly IMqttEntityManager _entityManager;
        private readonly Serilog.ILogger _logger;

        private readonly IList<string> _entities = new List<string>();

        /// <summary>Initializes a new instance of <see cref="EntityService"/>.</summary>
        /// <param name="entityManager">The MQTT entity manager used to create entities and set their availability.</param>
        /// <param name="logger">The Serilog logger, scoped to <see cref="EntityService"/>.</param>
        public EntityService(IMqttEntityManager entityManager, Serilog.ILogger logger)
        {
            _logger = logger.ForContext<EntityService>();
            _entityManager = entityManager;
        }

        /// <inheritdoc/>
        public async Task Create(EntityDefinition entityDefinition, DeviceDefinition? deviceDefinition = null)
        {
            var options = new EntityCreationOptions()
            {
                Name = entityDefinition.FriendlyName,
                DeviceClass = entityDefinition.DeviceClass,
                PayloadAvailable = AVAILABLE,
                PayloadNotAvailable = NOT_AVAILABLE,
            };

            var additionalConfig = entityDefinition.BuildAdditionalConfig(deviceDefinition);

            await _entityManager.CreateAsync(entityDefinition.EntityId, options, additionalConfig);

            _entities.Add(entityDefinition.EntityId);
        }

        /// <inheritdoc/>
        public async Task Enable(string entityId)
        {
            _logger.Verbose($"Enabling [{entityId}]");
            await _entityManager.SetAvailabilityAsync(entityId, AVAILABLE).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public async Task Disable(string entityId)
        {
            _logger.Verbose($"Disabling [{entityId}]");
            await _entityManager.SetAvailabilityAsync(entityId, NOT_AVAILABLE).ConfigureAwait(false);
        }

        /// <summary>Disables every entity this service has created, in the order they were created.</summary>
        public async ValueTask DisposeAsync()
        {
            foreach (var entity in _entities)
            {
                _logger.Debug($"Dispose->Disabling [{entity}]");
                await Disable(entity).ConfigureAwait(false);
            }
        }
    }
}
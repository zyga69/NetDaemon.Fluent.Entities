using NetDaemon.Fluent.Entities.DependencyInjection;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// Resolves the <see cref="IEntityInitializer"/> registered (via <c>AddEntityInitializers</c>) for a given
    /// <see cref="Maps.Roots.EntityDefinition"/> subclass, so <c>DeviceService</c> can dispatch each entity to
    /// the initializer that knows how to create it.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve keyed initializer services.</param>
    internal class EntityInitializerFactory(IServiceProvider serviceProvider)
      : TypeKeyedServiceFactory<IEntityInitializer>(serviceProvider), IEntityInitializerFactory
    {
    }

    /// <summary>Scoped factory for resolving an <see cref="IEntityInitializer"/> by its registered definition type.</summary>
    internal interface IEntityInitializerFactory :
       ITypeKeyedServiceFactory<IEntityInitializer>,
       IScopedService
    {
    }
}
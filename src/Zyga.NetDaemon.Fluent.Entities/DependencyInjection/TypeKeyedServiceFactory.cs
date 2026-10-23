using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Fluent.Entities.DependencyInjection
{
    /// <summary>
    /// Base <see cref="ITypeKeyedServiceFactory{TService}"/> implementation that resolves a keyed service
    /// from <see cref="IServiceProvider"/> (keyed by CLR type, unwrapping <see cref="Nullable{T}"/> to its
    /// underlying type) and caches each resolved instance for subsequent lookups.
    /// </summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    internal abstract class TypeKeyedServiceFactory<TService> : ITypeKeyedServiceFactory<TService>
    {
        private readonly IDictionary<Type, TService> _services = new Dictionary<Type, TService>();
        private readonly IServiceProvider _serviceProvider;

        /// <summary>Initializes a new instance of <see cref="TypeKeyedServiceFactory{TService}"/>.</summary>
        /// <param name="serviceProvider">The service provider the keyed service is resolved from.</param>
        public TypeKeyedServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public TService GetService<TType>() => GetService(typeof(TType));

        /// <inheritdoc/>
        public TService GetService(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (_services.TryGetValue(underlyingType, out var service))
                return service;

            service = _serviceProvider.GetKeyedService<TService>(underlyingType);

            if (service == null)
                throw new Exception($"No service [{typeof(TService)}] found for type {type.Name}");

            _services.Add(underlyingType, service);

            return service;
        }
    }
}

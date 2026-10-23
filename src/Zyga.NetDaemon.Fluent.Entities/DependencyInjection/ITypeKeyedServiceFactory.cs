namespace NetDaemon.Fluent.Entities.DependencyInjection
{
    /// <summary>Resolves a <typeparamref name="TService"/> instance registered as a keyed service, keyed by a CLR <see cref="Type"/>.</summary>
    /// <typeparam name="TService">The service type to resolve.</typeparam>
    internal interface ITypeKeyedServiceFactory<TService>
    {
        /// <summary>Resolves the <typeparamref name="TService"/> registered under the given key type.</summary>
        /// <param name="type">The CLR type the service is keyed by.</param>
        TService GetService(Type type);

        /// <summary>Resolves the <typeparamref name="TService"/> registered under the key type <typeparamref name="TType"/>.</summary>
        /// <typeparam name="TType">The CLR type the service is keyed by.</typeparam>
        TService GetService<TType>();
    }
}

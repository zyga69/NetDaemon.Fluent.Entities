using NetDaemon.Fluent.Entities.DependencyInjection;

namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>
    /// Resolves the <see cref="ITypeConverter"/> registered (via <c>AddTypeConverters</c>) for a given CLR
    /// type, so initializers can convert device model values to/from MQTT sensor strings regardless of type.
    /// </summary>
    /// <param name="serviceProvider">The service provider used to resolve keyed converter services.</param>
    internal class ConverterFactory(IServiceProvider serviceProvider)
        : TypeKeyedServiceFactory<ITypeConverter>(serviceProvider), IConverterFactory
    {
    }

    /// <summary>Scoped factory for resolving an <see cref="ITypeConverter"/> by the CLR type it converts.</summary>
    internal interface IConverterFactory :
        ITypeKeyedServiceFactory<ITypeConverter>,
        IScopedService
    {
    }
}
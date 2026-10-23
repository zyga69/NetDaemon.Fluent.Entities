using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Fluent.Entities.Converters
{
    /// <summary>DI registration helpers for the built-in <see cref="ITypeConverter"/> implementations.</summary>
    public static class RegistrationsExtensions
    {
        /// <summary>
        /// Registers the built-in <see cref="ITypeConverter"/>s (for <see cref="int"/>, <see cref="double"/>,
        /// <see cref="string"/>, <see cref="bool"/>, <see cref="DateTime"/>) as scoped services keyed by the
        /// CLR type they convert, so <see cref="IConverterFactory"/> can resolve them. A consuming host must
        /// call this (alongside <c>AddEntityInitializers</c>) when registering the library's services.
        /// </summary>
        /// <param name="services">The service collection to register into.</param>
        /// <returns>The same service collection, for chaining.</returns>
        public static IServiceCollection AddTypeConverters(this IServiceCollection services)
        {
            return services
                .AddKeyedScoped<ITypeConverter, IntConverter>(typeof(int))
                .AddKeyedScoped<ITypeConverter, DoubleConverter>(typeof(double))
                .AddKeyedScoped<ITypeConverter, StringConverter>(typeof(string))
                .AddKeyedScoped<ITypeConverter, BooleanConverter>(typeof(bool))
                .AddKeyedScoped<ITypeConverter, DateTimeConverter>(typeof(DateTime));
        }
    }
}
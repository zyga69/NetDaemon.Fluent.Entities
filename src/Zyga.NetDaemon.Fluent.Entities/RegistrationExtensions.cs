using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;

namespace NetDaemon.Fluent.Entities
{
    /// <summary>DI registration extensions for the library's own services.</summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Registers the built-in <see cref="IEntityInitializer"/> implementations as scoped services keyed
        /// by the <c>EntityDefinition</c> subclass they handle (<c>TemplateSensorDefinition</c> &#8594; <see cref="TemplateSensorService"/>,
        /// <c>SensorDefinition</c> &#8594; <see cref="SensorService"/>, <c>ActionDefinition</c> &#8594; <see cref="ButtonService"/>,
        /// <c>SwitchDefinition</c> &#8594; <see cref="SwitchService"/>), so <see cref="DeviceService"/> can dispatch each mapped
        /// entity to the right one. Call this once from the consuming host's DI setup.
        /// </summary>
        /// <param name="services">The service collection to register against.</param>
        public static IServiceCollection AddEntityInitializers(this IServiceCollection services)
        {
            return services
                .AddKeyedScoped<IEntityInitializer, TemplateSensorService>(typeof(TemplateSensorDefinition))
                .AddKeyedScoped<IEntityInitializer, SensorService>(typeof(SensorDefinition))
                .AddKeyedScoped<IEntityInitializer, ButtonService>(typeof(ActionDefinition))
                .AddKeyedScoped<IEntityInitializer, SwitchService>(typeof(SwitchDefinition));
        }
    }
}
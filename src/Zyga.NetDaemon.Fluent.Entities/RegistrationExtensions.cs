using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;
using System.Reflection;

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

        /// <summary>
        /// Registers everything the library needs in one call: <see cref="AddEntityInitializers"/> and
        /// <see cref="Converters.RegistrationsExtensions.AddTypeConverters"/>, the library's own internal
        /// services, and - discovered by scanning <paramref name="appAssemblies"/> - every
        /// <see cref="Maps.DeviceMap{TDevice}"/> subclass together with its device model type. Each device
        /// model is registered both as a plain service and as a service keyed by any key, so it resolves
        /// whether its map used <c>AsDevice(identifier)</c> or <c>AsDevice(identifier, key)</c> - the actual
        /// key value is only known once the map's constructor runs, long after this method returns, so it
        /// can't be matched in advance; registering under <see cref="KeyedService.AnyKey"/> sidesteps that.
        /// </summary>
        /// <param name="services">The service collection to register against.</param>
        /// <param name="appAssemblies">
        /// The assemblies to scan for <see cref="Maps.DeviceMap{TDevice}"/> subclasses - typically just your
        /// own app's assembly (e.g. <c>Assembly.GetExecutingAssembly()</c>).
        /// </param>
        /// <exception cref="ArgumentException">No assemblies were given.</exception>
        public static IServiceCollection AddFluentEntities(this IServiceCollection services, params Assembly[] appAssemblies)
        {
            ArgumentNullException.ThrowIfNull(appAssemblies);

            if (appAssemblies.Length == 0)
                throw new ArgumentException("At least one assembly containing DeviceMap<TDevice> subclasses must be provided.", nameof(appAssemblies));

            services
                .AddEntityInitializers()
                .AddTypeConverters();

            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<IEntityService, EntityService>();
            services.AddScoped<IDeviceRegistry, DeviceRegistry>();
            services.AddScoped<ISensorService, SensorService>();
            services.AddScoped<IButtonService, ButtonService>();
            services.AddScoped<ISwitchService, SwitchService>();
            services.AddScoped<ITemplateSensorService, TemplateSensorService>();
            services.AddScoped<IEntityInitializerFactory, EntityInitializerFactory>();
            services.AddScoped<IConverterFactory, ConverterFactory>();

            var deviceModelTypes = new HashSet<Type>();

            foreach (var mapType in DiscoverDeviceMapTypes(appAssemblies))
            {
                services.AddScoped(mapType);

                var deviceType = GetDeviceModelType(mapType);
                if (deviceType != null)
                    deviceModelTypes.Add(deviceType);
            }

            foreach (var deviceType in deviceModelTypes)
            {
                services.AddScoped(deviceType, sp => ActivatorUtilities.CreateInstance(sp, deviceType));
                services.AddKeyedScoped(deviceType, KeyedService.AnyKey, (sp, _) => ActivatorUtilities.CreateInstance(sp, deviceType));
            }

            return services;
        }

        private static IEnumerable<Type> DiscoverDeviceMapTypes(IEnumerable<Assembly> assemblies)
        {
            return assemblies
                .Distinct()
                .SelectMany(a => a.GetTypes())
                .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IDeviceMap).IsAssignableFrom(t));
        }

        /// <summary>
        /// Walks up <paramref name="mapType"/>'s base type chain to find its closed <see cref="Maps.DeviceMap{TDevice}"/>
        /// base and returns the <c>TDevice</c> it was closed over, or <c>null</c> if <paramref name="mapType"/>
        /// implements <see cref="IDeviceMap"/> without deriving from <see cref="Maps.DeviceMap{TDevice}"/>.
        /// </summary>
        private static Type? GetDeviceModelType(Type mapType)
        {
            for (var current = mapType; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(DeviceMap<>))
                    return current.GetGenericArguments()[0];
            }

            return null;
        }
    }
}
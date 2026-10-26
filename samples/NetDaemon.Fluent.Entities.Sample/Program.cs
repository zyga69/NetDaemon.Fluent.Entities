using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Fluent.Entities;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Fluent.Entities.Sample.Model;
using NetDaemon.Runtime;
using Scrutor;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();
var libraryAssembly = typeof(IDeviceService).Assembly;

var hostBuilder = Host.CreateDefaultBuilder(args)
    .UseNetDaemonAppSettings()
    .UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .MinimumLevel.Debug()
        .WriteTo.Console(theme: SystemConsoleTheme.Colored, applyThemeToRedirectedOutput: true))
    .ConfigureServices(services =>
    {
        services
            .AddNetDaemonRuntime()
            .AddNetDaemonMqttEntityManagement()
            .AddNetDaemonStateManager()
            .AddNetDaemonScheduler()
            .AddAppsFromAssembly(assembly)
            .AddEntityInitializers()
            .AddTypeConverters();

        // The device MODEL type itself must be a registered service too — IDeviceService
        // resolves it with a plain GetRequiredService(deviceDefinition.Type) whenever a
        // template sensor recomputes its value, not just the DeviceMap<TDevice> that declares it.
        services.AddScoped<SampleDevice>();

        // Discovers the library's own services (DeviceService, EntityService, DeviceRegistry)
        // from the library assembly, and every DeviceMap<TDevice> subclass (SampleDeviceMap)
        // from this sample's own assembly, via the library's DI marker interfaces, as
        // documented in the "Wiring the library into a host" README section.
        services.Scan(scan => scan
            .FromAssemblies(libraryAssembly, assembly)
            .AddClasses(c => c.AssignableTo<ISingleService>(), publicOnly: false)
                .AsImplementedInterfaces().AsSelf().WithSingletonLifetime()
            .AddClasses(c => c.AssignableTo<IScopedService>(), publicOnly: false)
                .AsImplementedInterfaces().AsSelf().WithScopedLifetime());
    });

using IHost host = hostBuilder.Build();

await host.RunAsync();

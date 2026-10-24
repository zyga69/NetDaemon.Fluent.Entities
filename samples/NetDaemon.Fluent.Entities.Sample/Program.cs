using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Fluent.Entities;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Runtime;
using Scrutor;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using System.Reflection;

var assembly = Assembly.GetExecutingAssembly();

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

        // Discovers the library's own services (DeviceService, EntityService, DeviceRegistry,
        // and every DeviceMap<TDevice> subclass, including SampleDeviceMap) via its DI marker
        // interfaces, as documented in the "Wiring the library into a host" README section.
        services.Scan(scan => scan
            .FromAssemblies(assembly)
            .AddClasses(c => c.AssignableTo<ISingleService>())
                .AsImplementedInterfaces().AsSelf().WithSingletonLifetime()
            .AddClasses(c => c.AssignableTo<IScopedService>())
                .AsImplementedInterfaces().AsSelf().WithScopedLifetime());
    });

using IHost host = hostBuilder.Build();

await host.RunAsync();

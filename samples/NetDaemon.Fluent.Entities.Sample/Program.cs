using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetDaemon.AppModel;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Extensions.Scheduler;
using NetDaemon.Fluent.Entities;
using NetDaemon.Runtime;
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
            .AddFluentEntities(assembly);
    });

using IHost host = hostBuilder.Build();

await host.RunAsync();

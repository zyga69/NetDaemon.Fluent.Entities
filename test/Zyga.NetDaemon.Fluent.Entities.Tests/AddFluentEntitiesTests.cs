using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;
using NetDaemon.Fluent.Entities.Model;
using NSubstitute;
using Serilog;
using System.Reactive.Concurrency;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class FluentEntitiesTestDep
    {
        public string Name => "injected-dep";
    }

    public class FluentEntitiesTestDevice(FluentEntitiesTestDep dep)
    {
        public FluentEntitiesTestDep Dep { get; } = dep;
    }

    public class FluentEntitiesTestDeviceMap : DeviceMap<FluentEntitiesTestDevice>
    {
        public FluentEntitiesTestDeviceMap()
        {
            AsDevice("fluent_entities_test_device").WithFriendlyName("Test Device");
        }
    }

    // Maps the same device model as FluentEntitiesTestDeviceMap, to prove two maps sharing one
    // device type don't cause a duplicate-registration conflict when the device type is registered.
    public class FluentEntitiesTestDeviceMapAlt : DeviceMap<FluentEntitiesTestDevice>
    {
        public FluentEntitiesTestDeviceMapAlt()
        {
            AsDevice("fluent_entities_test_device_alt").WithFriendlyName("Test Device Alt");
        }
    }

    // Implements IDeviceMap directly instead of deriving from DeviceMap<TDevice>, which the library
    // does not support - every real DeviceMap<TDevice> subclass does derive from the generic base.
    public class FluentEntitiesBadMap : IDeviceMap
    {
        public DeviceDefinition Definition => new("bad", typeof(object));
    }

    public class AddFluentEntitiesTests
    {
        private static ServiceCollection BuildServices(params Assembly[] assemblies)
        {
            var services = new ServiceCollection();
            services.AddSingleton(Substitute.For<ILogger>());
            services.AddSingleton(Substitute.For<IMqttEntityManager>());
            services.AddSingleton(Substitute.For<IScheduler>());
            services.AddSingleton<FluentEntitiesTestDep>();

            services.AddFluentEntities(assemblies);

            return services;
        }

        [Fact]
        public void AddFluentEntities_ResolvesIDeviceService()
        {
            var provider = BuildServices(typeof(AddFluentEntitiesTests).Assembly).BuildServiceProvider();

            var deviceService = provider.GetRequiredService<IDeviceService>();

            Assert.IsType<DeviceService>(deviceService);
        }

        [Fact]
        public void AddFluentEntities_DiscoversAndRegistersDeviceMapFromGivenAssembly()
        {
            var provider = BuildServices(typeof(AddFluentEntitiesTests).Assembly).BuildServiceProvider();

            var map = provider.GetRequiredService<FluentEntitiesTestDeviceMap>();

            Assert.NotNull(map);
        }

        [Fact]
        public void AddFluentEntities_RegistersDeviceModelAsPlainService()
        {
            var provider = BuildServices(typeof(AddFluentEntitiesTests).Assembly).BuildServiceProvider();

            var device = (FluentEntitiesTestDevice)provider.GetRequiredService(typeof(FluentEntitiesTestDevice));

            Assert.Equal("injected-dep", device.Dep.Name);
        }

        [Fact]
        public void AddFluentEntities_RegistersDeviceModelAsKeyedServiceForAnyKey()
        {
            var provider = BuildServices(typeof(AddFluentEntitiesTests).Assembly).BuildServiceProvider();

            var device = (FluentEntitiesTestDevice)provider.GetRequiredKeyedService(typeof(FluentEntitiesTestDevice), "a-key-never-registered-explicitly");

            Assert.Equal("injected-dep", device.Dep.Name);
        }

        [Fact]
        public void AddFluentEntities_TwoMapsSharingSameDeviceModel_BuildsAndResolvesBoth()
        {
            var provider = BuildServices(typeof(AddFluentEntitiesTests).Assembly).BuildServiceProvider();

            var mapA = provider.GetRequiredService<FluentEntitiesTestDeviceMap>();
            var mapB = provider.GetRequiredService<FluentEntitiesTestDeviceMapAlt>();

            Assert.NotNull(mapA);
            Assert.NotNull(mapB);
        }

        [Fact]
        public void AddFluentEntities_NoAssembliesGiven_Throws()
        {
            var services = new ServiceCollection();

            Assert.Throws<ArgumentException>(() => services.AddFluentEntities());
        }

        [Fact]
        public void AddFluentEntities_ReturnsSameServiceCollectionForChaining()
        {
            var services = new ServiceCollection();

            var result = services.AddFluentEntities(typeof(AddFluentEntitiesTests).Assembly);

            Assert.Same(services, result);
        }

        [Fact]
        public void AddFluentEntities_StillRegistersKeyedEntityInitializers()
        {
            var services = new ServiceCollection();

            services.AddFluentEntities(typeof(AddFluentEntitiesTests).Assembly);

            var descriptor = Assert.Single(services, d =>
                d.ServiceType == typeof(IEntityInitializer) &&
                Equals(d.ServiceKey, typeof(TemplateSensorDefinition)));

            Assert.Equal(typeof(TemplateSensorService), descriptor.KeyedImplementationType);
        }

        [Fact]
        public void AddFluentEntities_TypeImplementingIDeviceMapWithoutGenericBase_RegistersMapButSkipsDeviceModel()
        {
            // FluentEntitiesBadMap implements IDeviceMap directly rather than deriving from
            // DeviceMap<TDevice>, which is not how the library is meant to be used, but scanning
            // must not crash the whole registration over one such type - it just can't infer a
            // device model for it, so it registers the map and skips auto-registering a device model.
            var services = BuildServices(typeof(AddFluentEntitiesTests).Assembly);

            Assert.DoesNotContain(services, d => d.ServiceType == typeof(object) && d.Lifetime == ServiceLifetime.Scoped);

            var provider = services.BuildServiceProvider();
            var badMap = provider.GetRequiredService<FluentEntitiesBadMap>();
            Assert.NotNull(badMap);
        }
    }
}

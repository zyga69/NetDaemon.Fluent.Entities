using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Switch;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NSubstitute;
using Serilog;
using System.Reactive.Subjects;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class SwitchServiceTestDevice
    {
        public bool IsOn { get; set; }
    }

    public class SwitchServiceTestMap : DeviceMap<SwitchServiceTestDevice>
    {
    }

    public class SwitchServiceTests
    {
        private readonly IEntityService _entityService;
        private readonly IMqttEntityManager _entityManager;
        private readonly IConverterFactory _converterFactory;
        private readonly ITypeConverter _typeConverter;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private readonly SwitchService _switchService;

        public SwitchServiceTests()
        {
            _entityService = Substitute.For<IEntityService>();
            _entityManager = Substitute.For<IMqttEntityManager>();
            _converterFactory = Substitute.For<IConverterFactory>();
            _typeConverter = Substitute.For<ITypeConverter>();
            _converterFactory.GetService<bool>().Returns(_typeConverter);
            _serviceProvider = Substitute.For<IServiceProvider>();
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<SwitchService>().Returns(_logger);

            // SwitchService resolves its converter eagerly in its primary constructor, so the stub above
            // must be set up before this call.
            _switchService = new SwitchService(_entityService, _entityManager, _converterFactory, _serviceProvider, _logger);
        }

        /// <summary>Builds a map with a single device and a single bool-backed switch, mirroring the fluent API usage shown in DeviceMapTests.</summary>
        private static (SwitchServiceTestMap Map, SwitchDefinition Definition, SwitchMapRoot Root) BuildMap()
        {
            var map = new SwitchServiceTestMap();
            map.AsDevice(StringHelpers.Random()).WithFriendlyName(StringHelpers.Random());

            var root = map.Map(x => x.IsOn, $"switch.{map.Definition.Identifier}_{StringHelpers.Random()}")
                .AsSwitch();

            var definition = (SwitchDefinition)map.Definition.Entities.Single();

            return (map, definition, root);
        }

        [Fact]
        public async Task InitWithDevice_WithNullDefinition_ThrowsArgumentNullException()
        {
            var (map, _, _) = BuildMap();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _switchService.InitWithDevice(null!, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_WithNullDeviceDefinition_ThrowsArgumentNullException()
        {
            var (_, definition, _) = BuildMap();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _switchService.InitWithDevice(definition, null!));
        }

        [Fact]
        public async Task InitWithDevice_WithWrongDefinitionType_ThrowsArgumentException()
        {
            var (map, _, _) = BuildMap();

            var property = typeof(SwitchServiceTestDevice).GetProperty(nameof(SwitchServiceTestDevice.IsOn));
            ArgumentNullException.ThrowIfNull(property);
            var wrongDefinition = new SensorDefinition(StringHelpers.Random(), "sensor", map.Definition.Identifier, property);

            await Assert.ThrowsAsync<ArgumentException>(() => _switchService.InitWithDevice(wrongDefinition, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_DefaultInitialState_CreatesSubscribesPublishesStateAndEnables()
        {
            var (map, definition, _) = BuildMap(); // InitialState defaults to false (off)

            _typeConverter.ToSensorString(false).Returns("OFF");
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);

            await _entityService.Received(1).Create(definition, map.Definition);
            await _entityManager.Received(1).PrepareCommandSubscriptionAsync(definition.EntityId);
            await _entityManager.Received(1).SetStateAsync(definition.EntityId, "OFF");
            await _entityService.Received(1).Enable(definition.EntityId);
        }

        [Fact]
        public async Task InitWithDevice_InitiallyOn_PublishesConvertedOnState()
        {
            var (map, definition, root) = BuildMap();
            root.InitiallyOn();

            _typeConverter.ToSensorString(true).Returns("ON");
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);

            await _entityManager.Received(1).SetStateAsync(definition.EntityId, "ON");
        }

        [Fact]
        public async Task InitWithDevice_WhenInitialStateConverterReturnsNull_SkipsPublishingInitialState()
        {
            var (map, definition, _) = BuildMap();

            _typeConverter.ToSensorString(false).Returns((string?)null);
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);

            await _entityManager.DidNotReceive().SetStateAsync(Arg.Any<string>(), Arg.Any<string>());
            await _entityService.Received(1).Enable(definition.EntityId);
        }

        [Fact]
        public async Task InitWithDevice_WhenCommandReceived_RepublishesReceivedStateAsIs()
        {
            var (map, definition, _) = BuildMap();
            var device = new SwitchServiceTestDevice();

            _typeConverter.ToSensorString(false).Returns("OFF");
            _serviceProvider.GetService(typeof(SwitchServiceTestDevice)).Returns(device);
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);

            subject.OnNext("ON");

            await _entityManager.Received(1).SetStateAsync(definition.EntityId, "ON");
        }

        [Fact]
        public async Task InitWithDevice_WhenCommandReceived_WritesConvertedValueOntoDeviceModelProperty()
        {
            var (map, definition, _) = BuildMap();
            var device = new SwitchServiceTestDevice { IsOn = false };

            _typeConverter.ToSensorString(false).Returns("OFF");
            _typeConverter.FromSensorString("ON").Returns(true);
            _serviceProvider.GetService(typeof(SwitchServiceTestDevice)).Returns(device);
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);

            subject.OnNext("ON");

            Assert.True(device.IsOn);
        }

        [Fact]
        public async Task InitWithDevice_WhenCommandCannotBeConverted_LeavesDeviceModelPropertyUnchanged()
        {
            var (map, definition, _) = BuildMap();
            var device = new SwitchServiceTestDevice { IsOn = false };

            _typeConverter.ToSensorString(false).Returns("OFF");
            _typeConverter.FromSensorString("not-a-bool").Returns((object?)null);
            _serviceProvider.GetService(typeof(SwitchServiceTestDevice)).Returns(device);
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);

            subject.OnNext("not-a-bool");

            Assert.False(device.IsOn);
        }

        [Fact]
        public async Task Dispose_UnsubscribesCommandSubscription()
        {
            var (map, definition, _) = BuildMap();

            _typeConverter.ToSensorString(false).Returns("OFF");
            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(definition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _switchService.InitWithDevice(definition, map.Definition);
            Assert.True(subject.HasObservers);

            _switchService.Dispose();

            Assert.False(subject.HasObservers);
        }
    }
}

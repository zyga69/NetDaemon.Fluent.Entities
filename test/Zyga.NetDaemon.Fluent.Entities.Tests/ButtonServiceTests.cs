using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Maps.Roots.Button;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NSubstitute;
using Serilog;
using System.Reactive.Subjects;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class ButtonServiceTestDevice
    {
        public int PressCount { get; set; }

        public void Press() => PressCount++;
    }

    public class ButtonServiceTestMap : DeviceMap<ButtonServiceTestDevice>
    {
    }

    public class ButtonServiceTests
    {
        private readonly IEntityService _entityService;
        private readonly IMqttEntityManager _entityManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        private readonly ButtonService _buttonService;

        public ButtonServiceTests()
        {
            _entityService = Substitute.For<IEntityService>();
            _entityManager = Substitute.For<IMqttEntityManager>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            _logger = Substitute.For<ILogger>();
            _logger.ForContext<ButtonService>().Returns(_logger);

            _buttonService = new ButtonService(_entityService, _entityManager, _serviceProvider, _logger);
        }

        /// <summary>Builds a map with a single device and a single method-backed button, mirroring the fluent API usage shown in DeviceMapTests.</summary>
        private static (ButtonServiceTestMap Map, ActionDefinition ActionDefinition) BuildMap()
        {
            var map = new ButtonServiceTestMap();
            map.AsDevice(StringHelpers.Random()).WithFriendlyName(StringHelpers.Random());

            map.Map(x => x.Press(), $"button.{map.Definition.Identifier}_{StringHelpers.Random()}")
                .AsButton();

            var actionDefinition = map.Definition.Buttons.Single();

            return (map, actionDefinition);
        }

        [Fact]
        public async Task InitWithDevice_WithNullDefinition_ThrowsArgumentNullException()
        {
            var (map, _) = BuildMap();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _buttonService.InitWithDevice(null!, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_WithNullDeviceDefinition_ThrowsArgumentNullException()
        {
            var (_, actionDefinition) = BuildMap();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _buttonService.InitWithDevice(actionDefinition, null!));
        }

        [Fact]
        public async Task InitWithDevice_WithWrongDefinitionType_ThrowsArgumentException()
        {
            var (map, _) = BuildMap();

            var property = typeof(ButtonServiceTestDevice).GetProperty(nameof(ButtonServiceTestDevice.PressCount));
            ArgumentNullException.ThrowIfNull(property);
            var wrongDefinition = new SensorDefinition(StringHelpers.Random(), "sensor", map.Definition.Identifier, property);

            await Assert.ThrowsAsync<ArgumentException>(() => _buttonService.InitWithDevice(wrongDefinition, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_CreatesEntitySubscribesAndEnables()
        {
            var (map, actionDefinition) = BuildMap();

            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(actionDefinition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _buttonService.InitWithDevice(actionDefinition, map.Definition);

            await _entityService.Received(1).Create(actionDefinition, map.Definition);
            await _entityManager.Received(1).PrepareCommandSubscriptionAsync(actionDefinition.EntityId);
            await _entityService.Received(1).Enable(actionDefinition.EntityId);
        }

        [Fact]
        public async Task InitWithDevice_WhenButtonPressed_DisablesInvokesMethodThenEnables()
        {
            var (map, actionDefinition) = BuildMap();
            var device = new ButtonServiceTestDevice();

            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(actionDefinition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));
            _serviceProvider.GetService(typeof(ButtonServiceTestDevice)).Returns(device);

            await _buttonService.InitWithDevice(actionDefinition, map.Definition);

            subject.OnNext("PRESS");

            Assert.Equal(1, device.PressCount);
            await _entityService.Received(1).Disable(actionDefinition.EntityId);
            await _entityService.Received(2).Enable(actionDefinition.EntityId); // once after Init, once after the press
        }

        [Fact]
        public async Task Dispose_UnsubscribesCommandSubscription()
        {
            var (map, actionDefinition) = BuildMap();

            var subject = new Subject<string>();
            _entityManager.PrepareCommandSubscriptionAsync(actionDefinition.EntityId)
                .Returns(Task.FromResult<IObservable<string>>(subject));

            await _buttonService.InitWithDevice(actionDefinition, map.Definition);
            Assert.True(subject.HasObservers);

            _buttonService.Dispose();

            Assert.False(subject.HasObservers);
        }
    }
}

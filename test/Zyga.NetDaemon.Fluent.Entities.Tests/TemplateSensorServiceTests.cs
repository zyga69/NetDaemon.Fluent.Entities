using Microsoft.Reactive.Testing;
using NetDaemon.Extensions.MqttEntityManager;
using NetDaemon.Fluent.Entities.Converters;
using NetDaemon.Fluent.Entities.Initializers;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Maps.Roots.Sensor;
using NetDaemon.Fluent.Entities.Maps.Roots.Template;
using NetDaemon.Fluent.Entities.Tests.Helpers;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NSubstitute;
using Serilog;
using System.Reactive.Subjects;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Tests
{
    public class TemplateSensorTestDevice
    {
        public int CallCount { get; set; }
        public int ParameterlessResult { get; set; }

        public int Parameterless()
        {
            CallCount++;
            return ParameterlessResult;
        }

        public bool? WithArg(bool? arg)
        {
            CallCount++;
            return arg;
        }

        public int NonNullableArg(int arg)
        {
            CallCount++;
            return arg;
        }

        public int TwoParams(int a, int b)
        {
            CallCount++;
            return a + b;
        }
    }

    public class TemplateSensorTestMap : DeviceMap<TemplateSensorTestDevice>
    {
    }

    public class TemplateSensorServiceTests
    {
        private readonly IEntityService _entityService;
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConverterFactory _converterFactory;
        private readonly IMqttEntityManager _entityManager;
        private readonly TestScheduler _scheduler;
        private readonly IHaContext _haContext;
        private readonly Subject<StateChange> _stateAllChanges;

        private readonly TemplateSensorService _service;

        public TemplateSensorServiceTests()
        {
            _entityService = Substitute.For<IEntityService>();
            _logger = Substitute.For<ILogger>();
            _serviceProvider = Substitute.For<IServiceProvider>();
            _converterFactory = Substitute.For<IConverterFactory>();
            _entityManager = Substitute.For<IMqttEntityManager>();
            _scheduler = new TestScheduler();

            _haContext = Substitute.For<IHaContext>();
            _stateAllChanges = new Subject<StateChange>();
            _haContext.StateAllChanges().Returns(_stateAllChanges);

            _service = new TemplateSensorService(_entityService, _logger, _serviceProvider, _converterFactory, _entityManager, _scheduler);
        }

        private static TemplateSensorTestMap BuildMap()
        {
            var map = new TemplateSensorTestMap();
            map.AsDevice(StringHelpers.Random()).WithFriendlyName(StringHelpers.Random());
            return map;
        }

        private Entity CreateTriggerEntity(string? initialState)
        {
            string entityId = $"sensor.{StringHelpers.Random()}";
            _haContext.GetState(entityId).Returns(new EntityState { EntityId = entityId, State = initialState });
            return new Entity(_haContext, entityId);
        }

        private static StateChange MakeStateChange(Entity entity, string? oldState, string? newState)
        {
            var old = new EntityState { EntityId = entity.EntityId, State = oldState };
            var @new = new EntityState { EntityId = entity.EntityId, State = newState };
            return new StateChange(entity, old, @new);
        }

        [Fact]
        public async Task InitWithDevice_WithNullDefinition_ThrowsArgumentNullException()
        {
            var map = BuildMap();

            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.InitWithDevice(null!, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_WithWrongDefinitionType_ThrowsArgumentException()
        {
            var map = BuildMap();
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map(x => x.Parameterless(), entityId).AsTemplateSensor().TriggeredBy(triggerEntity);

            var property = typeof(TemplateSensorTestDevice).GetProperty(nameof(TemplateSensorTestDevice.CallCount));
            ArgumentNullException.ThrowIfNull(property);
            var wrongDefinition = new SensorDefinition(StringHelpers.Random(), "sensor", map.Definition.Identifier, property);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.InitWithDevice(wrongDefinition, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_MethodWithMoreThanOneParameter_ThrowsArgumentException()
        {
            var map = BuildMap();
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            var method = typeof(TemplateSensorTestDevice).GetMethod(nameof(TemplateSensorTestDevice.TwoParams));
            ArgumentNullException.ThrowIfNull(method);
            var definition = new TemplateSensorDefinition(entityId, "sensor", map.Definition.Identifier, method);
            definition.TriggeringEntities.Add(triggerEntity);
            map.Definition.Entities.Add(definition);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.InitWithDevice(definition, map.Definition));
        }

        [Fact]
        public async Task InitWithDevice_ParameterlessMethod_PublishesInitialStateAndReactsToTrigger()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice { ParameterlessResult = 1 };
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map(x => x.Parameterless(), entityId).AsTemplateSensor().TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var resultConverter = Substitute.For<ITypeConverter>();
            resultConverter.ToSensorString(1).Returns("1");
            resultConverter.ToSensorString(2).Returns("2");
            _converterFactory.GetService(typeof(int)).Returns(resultConverter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);

            // Initial state is pushed from the triggering entity's current state before any subscription fires.
            Assert.Equal(1, device.CallCount);
            await _entityManager.Received(1).SetStateAsync(entityId, "1");
            await _entityService.Received(2).Enable(entityId); // once from the initial push, once unconditionally at the end of Init

            device.ParameterlessResult = 2;
            _stateAllChanges.OnNext(MakeStateChange(triggerEntity, "idle", "active"));

            Assert.Equal(2, device.CallCount);
            await _entityManager.Received(1).SetStateAsync(entityId, "2");
        }

        [Fact]
        public async Task InitWithDevice_SingleParameterMethod_PassesConvertedTriggerStateToMethod()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice();
            var triggerEntity = CreateTriggerEntity("off");

            string entityId = $"binary_sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map((x, arg) => x.WithArg(arg), entityId).AsTemplateSensor().TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var converter = Substitute.For<ITypeConverter>();
            converter.FromSensorString("off").Returns(false);
            converter.FromSensorString("on").Returns(true);
            converter.ToSensorString(false).Returns("off");
            converter.ToSensorString(true).Returns("on");
            _converterFactory.GetService(typeof(bool?)).Returns(converter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);

            Assert.Equal(1, device.CallCount);
            await _entityManager.Received(1).SetStateAsync(entityId, "off");

            _stateAllChanges.OnNext(MakeStateChange(triggerEntity, "off", "on"));

            Assert.Equal(2, device.CallCount);
            await _entityManager.Received(1).SetStateAsync(entityId, "on");
        }

        [Fact]
        public async Task InitWithDevice_NonNullableArgumentCannotBeParsed_DoesNotInvokeMethod()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice();
            var triggerEntity = CreateTriggerEntity("not-a-number");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            var method = typeof(TemplateSensorTestDevice).GetMethod(nameof(TemplateSensorTestDevice.NonNullableArg));
            ArgumentNullException.ThrowIfNull(method);
            var definition = new TemplateSensorDefinition(entityId, "sensor", map.Definition.Identifier, method);
            definition.TriggeringEntities.Add(triggerEntity);
            map.Definition.Entities.Add(definition);

            var argConverter = Substitute.For<ITypeConverter>();
            argConverter.FromSensorString(Arg.Any<string?>()).Returns((object?)null);
            _converterFactory.GetService(typeof(int)).Returns(argConverter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);

            Assert.Equal(0, device.CallCount);
            await _entityService.DidNotReceive().Disable(Arg.Any<string>());
            await _entityManager.DidNotReceive().SetStateAsync(Arg.Any<string>(), Arg.Any<string>());
            // TemplateSensorService.InitWithDevice calls Enable unconditionally after its loop over the
            // triggering entities, even when no value was ever computed/published for this entity. This test
            // characterizes that pre-existing, known-questionable behavior - it is deliberately left
            // uncorrected (out of scope for this change) and this assertion is not an endorsement of it.
            await _entityService.Received(1).Enable(entityId);
        }

        [Fact]
        public async Task InitWithDevice_WhenComputedValueIsNull_DisablesEntityWithoutConverting()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice();
            var triggerEntity = CreateTriggerEntity(null);

            string entityId = $"binary_sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map((x, arg) => x.WithArg(arg), entityId).AsTemplateSensor().TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var converter = Substitute.For<ITypeConverter>();
            converter.FromSensorString((string?)null).Returns((object?)null);
            _converterFactory.GetService(typeof(bool?)).Returns(converter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);

            Assert.Equal(1, device.CallCount); // WithArg(null) is still invoked - the parameter is nullable
            await _entityService.Received(1).Disable(entityId);
            converter.DidNotReceive().ToSensorString(Arg.Any<object>());
        }

        [Fact]
        public async Task InitWithDevice_WhenComputedValueConvertsToBlankString_DisablesEntity()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice { ParameterlessResult = 42 };
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map(x => x.Parameterless(), entityId).AsTemplateSensor().TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var resultConverter = Substitute.For<ITypeConverter>();
            resultConverter.ToSensorString(42).Returns("   ");
            _converterFactory.GetService(typeof(int)).Returns(resultConverter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);

            await _entityService.Received(1).Disable(entityId);
            await _entityManager.DidNotReceive().SetStateAsync(entityId, Arg.Any<string>());
        }

        [Fact]
        public async Task InitWithDevice_Debounced_AppliesOnlyAfterStablePeriodElapses()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice { ParameterlessResult = 1 };
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map(x => x.Parameterless(), entityId).AsTemplateSensor()
                .StableForSeconds(5)
                .TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var resultConverter = Substitute.For<ITypeConverter>();
            resultConverter.ToSensorString(1).Returns("1");
            resultConverter.ToSensorString(2).Returns("2");
            _converterFactory.GetService(typeof(int)).Returns(resultConverter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);
            Assert.Equal(1, device.CallCount); // initial push is not debounced

            device.ParameterlessResult = 2;
            _stateAllChanges.OnNext(MakeStateChange(triggerEntity, "idle", "active"));

            Assert.Equal(1, device.CallCount); // timer has not elapsed yet

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(4).Ticks);
            Assert.Equal(1, device.CallCount); // still short of the 5s stable window

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks); // total 6s
            Assert.Equal(2, device.CallCount);
            await _entityManager.Received(1).SetStateAsync(entityId, "2");
        }

        [Fact]
        public async Task InitWithDevice_Debounced_RestartsTimerAndOnlyAppliesLastValue()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice { ParameterlessResult = 1 };
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map(x => x.Parameterless(), entityId).AsTemplateSensor()
                .StableForSeconds(5)
                .TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var resultConverter = Substitute.For<ITypeConverter>();
            resultConverter.ToSensorString(1).Returns("1");
            resultConverter.ToSensorString(2).Returns("2");
            resultConverter.ToSensorString(3).Returns("3");
            _converterFactory.GetService(typeof(int)).Returns(resultConverter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);
            Assert.Equal(1, device.CallCount);

            device.ParameterlessResult = 2;
            _stateAllChanges.OnNext(MakeStateChange(triggerEntity, "idle", "mid"));

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks); // before the stable window elapses

            device.ParameterlessResult = 3;
            _stateAllChanges.OnNext(MakeStateChange(triggerEntity, "mid", "final")); // restarts the debounce timer

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(3).Ticks); // only 3s since the restart
            Assert.Equal(1, device.CallCount); // the "mid" trigger never got applied

            _scheduler.AdvanceBy(TimeSpan.FromSeconds(2).Ticks); // total 5s since the restart
            Assert.Equal(2, device.CallCount); // only one additional invocation despite two triggers
            await _entityManager.Received(1).SetStateAsync(entityId, "3");
            await _entityManager.DidNotReceive().SetStateAsync(entityId, "2");
        }

        [Fact]
        public async Task Dispose_UnsubscribesFromAllTriggeringEntities()
        {
            var map = BuildMap();
            var device = new TemplateSensorTestDevice { ParameterlessResult = 1 };
            var triggerEntity = CreateTriggerEntity("idle");

            string entityId = $"sensor.{map.Definition.Identifier}_{StringHelpers.Random()}";
            map.Map(x => x.Parameterless(), entityId).AsTemplateSensor().TriggeredBy(triggerEntity);
            var definition = (TemplateSensorDefinition)map.Definition.Entities.Single();

            var resultConverter = Substitute.For<ITypeConverter>();
            resultConverter.ToSensorString(1).Returns("1");
            _converterFactory.GetService(typeof(int)).Returns(resultConverter);
            _serviceProvider.GetService(typeof(TemplateSensorTestDevice)).Returns(device);

            await _service.InitWithDevice(definition, map.Definition);
            Assert.True(_stateAllChanges.HasObservers);

            _service.Dispose();

            Assert.False(_stateAllChanges.HasObservers);
        }
    }
}

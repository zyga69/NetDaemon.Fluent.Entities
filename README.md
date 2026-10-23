# NetDaemon.Fluent.Entities

[![CI](https://github.com/zyga69/NetDaemon.Fluent.Entities/actions/workflows/ci.yml/badge.svg)](https://github.com/zyga69/NetDaemon.Fluent.Entities/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/Zyga.NetDaemon.Fluent.Entities.svg)](https://www.nuget.org/packages/Zyga.NetDaemon.Fluent.Entities)
[![Downloads](https://img.shields.io/nuget/dt/Zyga.NetDaemon.Fluent.Entities.svg)](https://www.nuget.org/packages/Zyga.NetDaemon.Fluent.Entities)
[![codecov](https://codecov.io/gh/zyga69/NetDaemon.Fluent.Entities/branch/main/graph/badge.svg)](https://codecov.io/gh/zyga69/NetDaemon.Fluent.Entities)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> An unofficial, community-maintained extension for [NetDaemon](https://netdaemon.xyz/) — not affiliated
> with, endorsed by, or published by the NetDaemon project.

A fluent device/entity mapping framework for NetDaemon apps. It sits on top of `IMqttEntityManager` and
lets an app describe a device model's properties and methods as Home Assistant entities, declaratively,
instead of calling the MQTT entity manager by hand.

## Install

```bash
dotnet add package Zyga.NetDaemon.Fluent.Entities
```

Targets .NET 10 only — this follows from `NetDaemon.HassModel`/`NetDaemon.Extensions.Mqtt` (the APIs this
library sits on top of), which ship `net10.0` builds only.

## The moving parts, at a glance

- A **device model** is a plain class (e.g. `Tank`) whose properties/methods compute the values you want to
  expose.
- A **`DeviceMap<TDevice>`** subclass declares, in its constructor, which members of `TDevice` map to which
  HA entities, using a fluent API: `AsDevice(...)`, then `Map(...)` chained with `With*()` and `As*()`.
- An app calls `IDeviceService.Init<TMap>()` once, typically from `IAsyncInitializable.InitializeAsync`.
  `DeviceService` walks the map's entities and dispatches each one to the matching `IEntityInitializer`,
  which creates the MQTT discovery entity and, for sensors and template sensors, keeps it updated.

```csharp
// TankApp.cs
public class TankApp : IAsyncInitializable
{
    private readonly IDeviceService _deviceService;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await _deviceService.Init<TankMap>();
    }
}
```

## Declaring a device: `AsDevice`

Every map starts by naming the HA device the entities below it will belong to:

```csharp
public class TankMap : DeviceMap<Model.Tank>
{
    public TankMap(Entities entities, IHaContext haContext, TankConfig config)
    {
        AsDevice("tank_nd")
            .WithFriendlyName("Tank");
        ...
    }
}
```

`AsDevice(identifier)` registers a single instance of `TDevice`. When several instances of the *same*
device model type need to exist side by side, use the keyed overload, `AsDevice(identifier, key)`, and
differentiate them later with `IDeviceRegistry.GetKeyed`/`IDeviceService.InitFromMap`/`IDeviceService.UpdateKeyed`:

```csharp
// Two DeviceMap<TDevice> instances mapping the same device model, keyed to tell them apart
public class VitotrolRadiatorCircuitMap : DeviceMap<VitotrolCircuit>
{
    public VitotrolRadiatorCircuitMap()
    {
        AsDevice("vitotrol_radiators", VitotrolGroup.Radiators)
            .WithFriendlyName("Vitotrol Radiators");
        ...
    }
}
```

## Mapping a member: `Map(...)`

`Map(...)` is overloaded by the device model member's shape. Each overload returns a typed fluent root
exposing only the `As*()` terminals that make sense for that type.

| Device model member | `Map(...)` overload | Returns |
|---|---|---|
| `int` property | `Map(Expression<Func<TDevice, int>>, string)` | `GenericPropertyRoot<TDevice, int>` |
| `string` property | `Map(Expression<Func<TDevice, string>>, string)` | `GenericPropertyRoot<TDevice, string>` |
| `double` property | `Map(Expression<Func<TDevice, double>>, string)` | `GenericPropertyRoot<TDevice, double>` |
| `DateTime` property | `Map(Expression<Func<TDevice, DateTime>>, string)` | `DateTimePropertyRoot<TDevice>` |
| `bool?` property | `Map(Expression<Func<TDevice, bool?>>, string)` | `BooleanPropertyRoot<TDevice>` |
| single-parameter `bool?` method | `Map(Expression<Func<TDevice, bool?, bool?>>, string)` | `SingleParameterMethodRoot<TDevice, bool?, bool?>` |
| parameterless `void` method | `Map(Expression<Action<TDevice>>, string)` | `ActionRoot` |

```csharp
// int property -> AsSensor()
Map(x => x.ModeId, "mode_id")
    .WithFriendlyName("Mode Id")
    .AsSensor()
        .WithId(92);

// DateTime property -> AsTimestamp()
Map(x => x.CurrentDate, "current_date")
    .WithFriendlyName("Current Date")
    .AsTimestamp()
        .WithId(5385);

// bool? property -> AsSwitch()
Map(x => x.Channel1, "channel1")
    .AsSwitch().InitiallyOn();

// parameterless action method -> AsButton()
Map(x => x.ToggleGate(), "gate")
    .WithFriendlyName("Front Gate")
    .AsButton();
```

(`string`/`double` properties follow the same `GenericPropertyRoot<TDevice, T>` shape as `int` above -
`.AsSensor()`/`.AsTemplateSensor()` - and `bool?` properties also support `.AsBinarySensor()` alongside
`.AsSwitch()`.)

The entity id's second argument (`"mode_id"`, `"channel1"`, ...) can be a short id or a fully qualified one
matching the terminal's HA domain prefix; either way it is composed with the device id into
`{prefix}.{deviceId}_{name}` and validated against `^[a-z0-9_]+\.[a-z0-9_]+$`.

### Template sensors: the single-parameter method and parameterless method cases

A model method with no parameters that returns `bool?` or `string` has exactly the same delegate type,
`Func<TDevice, bool?>`, as a lambda reading a `bool?` property - so C#'s overload resolution routes both
to the same `Map(Expression<Func<TDevice, bool?>>, string)` overload, reaching `AsTemplateSensor()` from
there the same as if it had been a property. (The reverse terminals - `AsSensor()`, `AsSwitch()`, etc. -
call `GetProperty()` internally and only work with an actual property access, not a method call; only
`AsTemplateSensor()`'s `GetMethodInfo()` accepts a method call.) The explicit single-parameter overload
(`Map(Expression<Func<TDevice, bool?, bool?>>, string)`) exists for methods that take the triggering
entity's current (converted) state as their argument, and is likewise completed with
`.AsTemplateSensor()` - except it returns `ISingleTriggerSensor`, so it accepts exactly one
`TriggeredBy(...)` rather than `Triggered.By(...)` chaining.

## The `As*()` terminals

Each `Map(...)` overload exposes a subset of these, matching what makes sense for the mapped member's type:

- **`AsSensor()`** - a plain HA `sensor`, no device class.

  ```csharp
  Map(x => x.ModeId, "mode_id")
      .WithFriendlyName("Mode Id")
      .AsSensor()
          .WithId(92);
  ```

- **`AsBinarySensor()`** - a HA `binary_sensor` backed directly by a `bool?` property (as opposed to a
  template sensor computed from a method).

- **`AsSwitch()`** - a HA `switch`; chain `InitiallyOn()`/`InitiallyOff()` to set its state when created.

  ```csharp
  Map(x => x.Channel1, "channel1")
      .AsSwitch().InitiallyOn();
  ```

- **`AsTemperatureSensor()`** - a HA `sensor` with the `temperature` device class, `°C` unit, and a
  thermometer icon applied automatically.

  ```csharp
  Map(x => x.ExhaustGasTemperature, "exhaust_gas_temp")
      .WithFriendlyName("Exhaust Gas Temperature")
      .AsTemperatureSensor()
          .WithId(5372);
  ```

- **`AsTimestamp()`** - a HA `sensor` with the `timestamp` device class.

  ```csharp
  Map(x => x.CurrentDate, "current_date")
      .WithFriendlyName("Current Date")
      .AsTimestamp()
          .WithId(5385);
  ```

- **`AsButton()`** - a HA `button` that invokes the mapped parameterless method when pressed.

  ```csharp
  Map(x => x.ToggleGate(), "gate")
      .WithFriendlyName("Front Gate")
      .AsButton();
  ```

- **`AsTemplateSensor()`** - a push-based sensor whose value is (re)computed by invoking the mapped method
  whenever a triggering entity changes state (see the next section):

  ```csharp
  var overloadUp =
      Map(x => x.OverloadUp(), "overload_up")
          .WithFriendlyName("Overload Up")
          .AsTemplateSensor()
          .TriggeredBy(entities.BinarySensor.SomeUpstreamSensor)
          .GetFullEntityId();
  ```

  The HA domain (`sensor` vs. `binary_sensor`) is inferred from the mapped method's return type: `bool`/
  `bool?` becomes `binary_sensor`, everything else becomes `sensor`.

`AsSensor()`/`AsTemperatureSensor()`/`AsTimestamp()`/`AsBinarySensor()` all support `WithId(int sensorId)`,
which tags the sensor with an external numeric data point id so that
`IDeviceService.Update(IEnumerable<IIdValue> dataPoints)` can route an externally-sourced value to the
right sensor without needing to know its HA entity id.

## Template sensor triggers and `StableForSeconds`

A template sensor does nothing until at least one triggering entity is registered - that's what actually
adds it to the device's entity list. Register one with `TriggeredBy(entity)`, or several by chaining
`Triggered.By(entity1).By(entity2)...`:

```csharp
// A sensor driven by four upstream (already-debounced) template sensors
Map(x => x.LevelFloatString(), "level_float")
    .WithFriendlyName("Level Float")
    .AsTemplateSensor()
    .Triggered
        .By(haContext.Entity(overloadUpStable))
        .By(haContext.Entity(maxUpStable))
        .By(haContext.Entity(lowUpStable))
        .By(haContext.Entity(minUpStable));
```

`StableForSeconds(n)` debounces the trigger: a state change only takes effect once the triggering entity's
state has remained unchanged for `n` seconds (a further change within that window restarts the wait). It
must be chained before `TriggeredBy`, since it switches the builder to the single-trigger shape:

```csharp
var maxUpStable =
    Map(x => x.MaxUpStable(), "max_up_stable")
        .WithFriendlyName("Max Up Stable")
        .AsTemplateSensor()
        .StableForSeconds(config.FloatSensorStableSeconds)
        .TriggeredBy(haContext.Entity(maxUp))
        .GetFullEntityId();
```

`TriggeredBy`/`Triggered.By(...)` return a builder exposing `GetFullEntityId()`, which returns the fully
composed entity id (e.g. `sensor.tank_nd_max_up_stable`) - letting you chain one template sensor as the
trigger of the next, as shown above (`max_up` -> `max_up_stable` -> `level_float`).

A template sensor's computed value that is `null` or blank disables the entity (marks it unavailable in
HA) rather than publishing an empty state.

## Shared attributes: `WithFriendlyName` and `WithIcon`

Available on every `Map(...)` root (chain them before the terminal `As*()` call) and on the device itself
(after `AsDevice(...)`):

```csharp
AsDevice("tank_nd")
    .WithFriendlyName("Tank");

Map(x => x.OverloadUp(), "overload_up")
    .WithFriendlyName("Overload Up")
    .WithIcon(MaterialIconKind.Water)  // any Material.Icons.MaterialIconKind value
    .AsTemplateSensor()
    .TriggeredBy(entities.BinarySensor.SomeUpstreamSensor);
```

`WithIcon` takes a `Material.Icons.MaterialIconKind` and renders it to the `mdi:...` string HA expects.
`AsTemperatureSensor()` sets its own thermometer icon unconditionally, so a `WithIcon` call chained before
it has no visible effect on that particular terminal - use `WithIcon` for the other terminals, which don't
set one of their own.

## Wiring the library into a host

`NetDaemon.Fluent.Entities` registers its own services through two extension methods that a consuming host
must call once, alongside its own DI setup:

```csharp
services
    .AddEntityInitializers()  // NetDaemon.Fluent.Entities.RegistrationExtensions
    .AddTypeConverters();     // NetDaemon.Fluent.Entities.Converters.RegistrationsExtensions
```

- **`AddEntityInitializers()`** registers the built-in `IEntityInitializer`s (`TemplateSensorService`,
  `SensorService`, `ButtonService`, `SwitchService`) as scoped services keyed by the `EntityDefinition`
  subclass they handle, so `DeviceService` can dispatch each mapped entity to the right one.
- **`AddTypeConverters()`** registers the built-in `ITypeConverter`s (`int`, `double`, `string`, `bool`,
  `DateTime`) as scoped services keyed by the CLR type they convert, so sensor/template-sensor values of any
  of those types can be converted to/from MQTT sensor strings.

Beyond those two calls, the library's own services - `DeviceService`, `EntityService`, `DeviceRegistry`,
every `DeviceMap<TDevice>` subclass, and the few services above - are discovered by marking them with the
library's own DI marker interfaces, `NetDaemon.Fluent.Entities.DependencyInjection.ISingleService` (singleton)
and `IScopedService` (scoped), and having the host's own DI container scan (e.g. via
[Scrutor](https://github.com/khellang/Scrutor)) register anything assignable to them:

```csharp
services.Scan(scan => scan
    .FromAssemblies(assembly)
    .AddClasses(c => c.AssignableTo<NetDaemon.Fluent.Entities.DependencyInjection.ISingleService>())
        .AsImplementedInterfaces().AsSelf().WithSingletonLifetime()
    .AddClasses(c => c.AssignableTo<NetDaemon.Fluent.Entities.DependencyInjection.IScopedService>())
        .AsImplementedInterfaces().AsSelf().WithScopedLifetime());
```

A `DeviceMap<TDevice>` subclass implements `IDeviceMap`, which extends this library's `IScopedService` - so
as long as the assembly containing it is included in that scan, it is resolvable by
`IDeviceService.Init<TMap>()` with no further registration.

## Requirements

- .NET 10
- `NetDaemon.HassModel` 26.36.0+
- `NetDaemon.Extensions.Mqtt` 26.36.0+

## Contributing

Issues and pull requests are welcome. See [CONTRIBUTING.md](CONTRIBUTING.md).

## License

[MIT](LICENSE)

## Version

This package uses [MinVer](https://github.com/adamralph/minver) for semantic versioning,
derived from `v`-prefixed git tags (e.g. `v1.2.3`). There is no `<Version>` element
anywhere in the source — maintainers cut a release by pushing a tag.

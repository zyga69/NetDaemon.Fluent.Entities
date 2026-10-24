using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Sample.Model;
using NetDaemon.HassModel;

namespace NetDaemon.Fluent.Entities.Sample.Map;

/// <summary>
/// Maps every member of <see cref="SampleDevice"/> to a HA entity, one per supported entity kind.
/// See the "API reference" section of the repo README for what each chained call does.
/// </summary>
public class SampleDeviceMap : DeviceMap<SampleDevice>
{
    public SampleDeviceMap(IHaContext haContext)
    {
        AsDevice("sample_device")
            .WithFriendlyName("Sample Device");

        // Generic sensor, tagged with an external numeric id so an external data source
        // (e.g. IDeviceService.Update(IEnumerable<IIdValue>)) can push a value without
        // knowing the HA entity id.
        Map(x => x.Mode, "mode")
            .WithFriendlyName("Mode")
            .AsSensor()
                .WithId(1);

        // Sensor with the "temperature" device class, °C unit, and a thermometer icon applied automatically.
        Map(x => x.Temperature, "temperature")
            .WithFriendlyName("Temperature")
            .AsTemperatureSensor();

        // Sensor with the "timestamp" device class.
        Map(x => x.LastSeen, "last_seen")
            .WithFriendlyName("Last Seen")
            .AsTimestamp();

        // binary_sensor backed directly by a bool? property.
        Map(x => x.IsOnline, "is_online")
            .WithFriendlyName("Is Online")
            .AsBinarySensor();

        // switch; InitiallyOn()/InitiallyOff() sets its state when first created.
        Map(x => x.Enabled, "enabled")
            .WithFriendlyName("Enabled")
            .AsSwitch()
            .InitiallyOn();

        // button; invokes the mapped parameterless method when pressed in HA.
        Map(x => x.Reset(), "reset")
            .WithFriendlyName("Reset")
            .AsButton();

        // Template sensor: push-based, recomputed whenever the triggering entity's state
        // changes. StableForSeconds debounces that trigger. Replace the trigger entity id
        // below with a real one from your HA instance.
        Map(x => x.ComputedStatus(), "computed_status")
            .WithFriendlyName("Computed Status")
            .AsTemplateSensor()
            .StableForSeconds(5)
            .TriggeredBy(haContext.Entity("sensor.sample_trigger"));
    }
}

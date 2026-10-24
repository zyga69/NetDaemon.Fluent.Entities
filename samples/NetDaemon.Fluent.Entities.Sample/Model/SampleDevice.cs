namespace NetDaemon.Fluent.Entities.Sample.Model;

/// <summary>
/// A device model touching every entity kind the library supports, so <see cref="Map.SampleDeviceMap"/>
/// doubles as a complete reference for the fluent mapping API.
/// </summary>
public class SampleDevice
{
    /// <summary>Backing value for the <c>sensor</c> entity. Mapped with <c>WithId(1)</c>.</summary>
    public string Mode { get; set; } = "idle";

    /// <summary>Backing value for the <c>temperature</c> device-class sensor.</summary>
    public double Temperature { get; set; } = 21.5;

    /// <summary>Backing value for the <c>timestamp</c> device-class sensor.</summary>
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;

    /// <summary>Backing value for the <c>binary_sensor</c>.</summary>
    public bool? IsOnline { get; set; } = true;

    /// <summary>Backing value for the <c>switch</c>.</summary>
    public bool? Enabled { get; set; } = true;

    /// <summary>Invoked by the mapped <c>button</c> entity when pressed.</summary>
    public void Reset()
    {
        Mode = "idle";
    }

    /// <summary>
    /// Computed by the template sensor whenever its triggering entity's state changes
    /// (see <c>TriggeredBy</c>/<c>StableForSeconds</c> in <see cref="Map.SampleDeviceMap"/>).
    /// </summary>
    public string ComputedStatus() => $"{Mode} @ {Temperature:F1}°C";
}

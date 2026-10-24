using NetDaemon.AppModel;
using NetDaemon.Fluent.Entities.Sample.Map;
using NetDaemon.Fluent.Entities.Sample.Model;

namespace NetDaemon.Fluent.Entities.Sample;

[NetDaemonApp]
public class SampleApp(IDeviceService deviceService) : IAsyncInitializable
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Registers the device and creates its MQTT discovery entities. Call this once,
        // typically from IAsyncInitializable.InitializeAsync as shown here.
        await deviceService.Init<SampleDeviceMap>();

        // The common real-world loop: recompute every non-template sensor from the device
        // model and push the result, e.g. on a timer or in response to an upstream event.
        // (Not actually scheduled here, since this sample has no real HA/MQTT connection.)
        await UpdateFromLiveModelAsync(deviceService);
    }

    private static Task UpdateFromLiveModelAsync(IDeviceService deviceService)
        => deviceService.Update<SampleDevice>();
}

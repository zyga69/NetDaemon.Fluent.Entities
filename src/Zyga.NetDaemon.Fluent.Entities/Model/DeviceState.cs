namespace NetDaemon.Fluent.Entities.Model
{
    /// <summary>The lifecycle state of a mapped device's entities, as tracked by <see cref="DeviceDefinition.State"/>.</summary>
    public enum DeviceState
    {
        /// <summary>The device has not yet been initialized through <c>IDeviceService.Init</c>/<c>InitFromMap</c>.</summary>
        Uninitialized,

        /// <summary>The device's entities are available in HA.</summary>
        Enabled,

        /// <summary>The device's entities have been marked unavailable in HA.</summary>
        Disabled
    }
}
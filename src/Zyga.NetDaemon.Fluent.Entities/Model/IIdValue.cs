namespace NetDaemon.Fluent.Entities.Model
{
    /// <summary>
    /// A single external data point: an identifier correlating it with a sensor's <c>SensorId</c> (set via
    /// <c>WithId(int)</c>), and its raw string value. Passed in bulk to <c>IDeviceService.Update</c>/
    /// <c>UpdateKeyed</c> to push values from an external data source onto the matching mapped sensors.
    /// </summary>
    public interface IIdValue
    {
        /// <summary>Gets or sets the identifier correlating this data point with a sensor's <c>SensorId</c>.</summary>
        int Id { get; set; }

        /// <summary>Gets or sets the data point's raw string value.</summary>
        string Value { get; set; }
    }
}
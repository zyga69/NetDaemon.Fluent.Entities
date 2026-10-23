using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Fluent.Entities.Maps;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities
{
    /// <summary>
    /// Resolves a <see cref="Maps.DeviceMap{TDevice}"/>, initializes its mapped HA entities, and routes
    /// subsequent sensor updates and enable/disable state changes to the device it registers.
    /// </summary>
    public interface IDeviceService : IScopedService
    {
        /// <summary>
        /// Resolves <typeparamref name="TMap"/> from DI and initializes its device, unless the device
        /// (or, for a keyed device, the device/key pair) is already registered, in which case this is a no-op.
        /// </summary>
        /// <typeparam name="TMap">The device map type to resolve and initialize.</typeparam>
        Task Init<TMap>()
            where TMap : IDeviceMap;

        /// <summary>
        /// Initializes the device described by an already-constructed map instance, unless the device
        /// (or, for a keyed device, the device/key pair) is already registered, in which case this is a no-op.
        /// </summary>
        /// <typeparam name="TMap">The device map type.</typeparam>
        /// <param name="map">The map instance describing the device to initialize.</param>
        Task InitFromMap<TMap>(TMap map)
            where TMap : IDeviceMap;

        /// <summary>Gets the external numeric sensor ids (set via <c>WithId(int)</c>) declared for the registered, non-keyed device of type <typeparamref name="TDevice"/>.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        public List<int> GetSensorIds<TDevice>()
            where TDevice : class;

        /// <summary>Gets the external numeric sensor ids (set via <c>WithId(int)</c>) declared for the registered, keyed device of type <typeparamref name="TDevice"/> identified by <paramref name="key"/>.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="key">The key identifying the device instance.</param>
        public List<int> GetSensorIds<TDevice>(object key)
            where TDevice : class;

        /// <summary>
        /// Updates the non-keyed device's sensors, resolving the device model from DI and matching each
        /// <paramref name="dataPoints"/> entry to the sensor declared with the same external id.
        /// </summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="dataPoints">The externally-sourced values to route to the matching sensors.</param>
        Task Update<TDevice>(IEnumerable<IIdValue> dataPoints)
           where TDevice : class;

        /// <summary>
        /// Updates the non-keyed device's sensors by resolving the device model from DI and recomputing
        /// each sensor's value directly from the model.
        /// </summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        Task Update<TDevice>()
           where TDevice : class;

        /// <summary>Updates the non-keyed device's sensors from the given device model instance.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="device">The device model instance to read sensor values from.</param>
        Task UpdateFromModel<TDevice>(TDevice device)
            where TDevice : class;

        /// <summary>
        /// Updates the keyed device's sensors, resolving the device model from DI by <paramref name="key"/>
        /// and matching each <paramref name="dataPoints"/> entry to the sensor declared with the same external id.
        /// </summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="key">The key identifying the device instance.</param>
        /// <param name="dataPoints">The externally-sourced values to route to the matching sensors.</param>
        Task UpdateKeyed<TDevice>(object key, IEnumerable<IIdValue> dataPoints)
            where TDevice : class;

        /// <summary>Updates the keyed device's sensors from the given device model instance.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="key">The key identifying the device instance.</param>
        /// <param name="device">The device model instance to read sensor values from.</param>
        Task UpdateKeyedFromModel<TDevice>(object key, TDevice device)
            where TDevice : class;

        /// <summary>Disables the non-keyed device of type <typeparamref name="TDevice"/>, marking all of its entities unavailable in HA.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        Task Disable<TDevice>() where TDevice : class;

        /// <summary>Disables the keyed device of type <typeparamref name="TDevice"/> identified by <paramref name="key"/>, marking all of its entities unavailable in HA.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="key">The key identifying the device instance.</param>
        Task DisableKeyed<TDevice>(object key) where TDevice : class;

        /// <summary>Enables the non-keyed device of type <typeparamref name="TDevice"/>, marking all of its entities available in HA.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        Task Enable<TDevice>() where TDevice : class;

        /// <summary>Enables the keyed device of type <typeparamref name="TDevice"/> identified by <paramref name="key"/>, marking all of its entities available in HA.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        /// <param name="key">The key identifying the device instance.</param>
        Task EnableKeyed<TDevice>(object key) where TDevice : class;

        /// <summary>Enables the non-keyed device of type <typeparamref name="TDevice"/> only if it is currently disabled; otherwise does nothing.</summary>
        /// <typeparam name="TDevice">The device model type.</typeparam>
        Task EnsureEnabled<TDevice>() where TDevice : class;
    }
}
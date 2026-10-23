using NetDaemon.Fluent.Entities.DependencyInjection;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <summary>
    /// Tracks which devices have been registered (via <see cref="DeviceService"/>), keyed by the device
    /// model's CLR type, and looks them back up by type and, for keyed devices, by key.
    /// </summary>
    internal interface IDeviceRegistry : IScopedService, IDictionary<Type, IList<DeviceDefinition>>
    {
        /// <summary>Records a device definition as registered under its model type.</summary>
        /// <param name="definition">The device definition to record.</param>
        void AddDefinition(DeviceDefinition definition);

        /// <summary>Gets the single registered, non-keyed device definition for model type <typeparamref name="T"/>.</summary>
        /// <typeparam name="T">The device model type.</typeparam>
        DeviceDefinition Get<T>();

        /// <summary>Gets the registered device definition for model type <typeparamref name="T"/> identified by <paramref name="key"/>.</summary>
        /// <typeparam name="T">The device model type.</typeparam>
        /// <param name="key">The key identifying the device instance.</param>
        DeviceDefinition GetKeyed<T>(object key)
            where T : class;


        /// <summary>Returns whether any device definition is registered for the given model type.</summary>
        /// <param name="deviceType">The device model type.</param>
        bool Exists(Type deviceType);

        /// <summary>Returns whether a device definition is registered for the given model type and key.</summary>
        /// <param name="deviceType">The device model type.</param>
        /// <param name="key">The key identifying the device instance.</param>
        bool ExistsKeyed(Type deviceType, object key);
    }
}
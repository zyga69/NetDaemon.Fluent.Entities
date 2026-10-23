using NetDaemon.Fluent.Entities.DependencyInjection;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Initializers
{
    /// <summary>
    /// Initializes HA <c>sensor</c>/<c>binary_sensor</c> entities for property-backed
    /// <see cref="Maps.Roots.Sensor.SensorDefinition"/> mappings, and pushes their values via <see cref="Update"/>.
    /// </summary>
    internal interface ISensorService : IEntityInitializer,
        IScopedService
    {
        /// <summary>
        /// Updates the specified property of a device with a new value.
        /// </summary>
        /// <typeparam name="TDevice">The type of the device.</typeparam>
        /// <param name="entityId">The ID of the entity to update.</param>
        /// <param name="property">The property to update.</param>
        /// <param name="device">The device instance.</param>
        /// <param name="value">The new value to set. If null, the property will be updated with the current value of the device.</param>
        /// <returns>A task that represents the asynchronous update operation.</returns>
        Task Update<TDevice>(string entityId, PropertyInfo property, TDevice device, string? value = null)
            where TDevice : class;
    }
}
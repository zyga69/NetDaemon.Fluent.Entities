using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Model;
using System;

namespace NetDaemon.Fluent.Entities.Extensions
{
    /// <summary>Extension methods on <see cref="IServiceProvider"/>.</summary>
    internal static class IServiceProviderExtensions
    {
        /// <summary>
        /// Resolves the device model instance for <paramref name="deviceDefinition"/>: a plain required
        /// service when the definition has no <see cref="DeviceDefinition.Key"/>, or a required keyed
        /// service keyed by it otherwise.
        /// </summary>
        /// <param name="serviceProvider">The service provider to resolve from.</param>
        /// <param name="deviceDefinition">The device definition describing which model type (and, if any, key) to resolve.</param>
        public static object GetDevice(this IServiceProvider serviceProvider, DeviceDefinition deviceDefinition)
        {
            ArgumentNullException.ThrowIfNull(deviceDefinition);

            if (deviceDefinition.Key == null)
                return serviceProvider.GetRequiredService(deviceDefinition.Type);

            return serviceProvider.GetRequiredKeyedService(deviceDefinition.Type, deviceDefinition.Key);
        }
    }
}
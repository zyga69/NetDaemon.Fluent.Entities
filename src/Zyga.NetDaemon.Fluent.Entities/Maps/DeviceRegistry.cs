using NetDaemon.Fluent.Entities.Common.Extensions;
using NetDaemon.Fluent.Entities.Model;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <inheritdoc/>
    internal class DeviceRegistry : Dictionary<Type, IList<DeviceDefinition>>, IDeviceRegistry
    {
        /// <inheritdoc/>
        public void AddDefinition(DeviceDefinition definition)
        {
            if (!ContainsKey(definition.Type))
                Add(definition.Type, []);

            this[definition.Type].Add(definition);
        }

        /// <inheritdoc/>
        public DeviceDefinition Get<TDevice>()
        {
            return GetList<TDevice>().Single();
        }



        /// <inheritdoc/>
        public DeviceDefinition GetKeyed<TDevice>(object key)
            where TDevice : class
        {
            ArgumentNullException.ThrowIfNull(key);


            var type = typeof(TDevice);

            var deviceDefinition = GetList<TDevice>()
                .Where(x => key.IsEqualTo(x.Key))
                .FirstOrDefault();

            if (deviceDefinition == null)
                throw new Exception($"Device [{type.Name}] with key [{key}] has not been registered");

            return deviceDefinition;
        }

        private IList<DeviceDefinition> GetList<TDevice>()
        {
            var type = typeof(TDevice);

            if (!ContainsKey(type))
                throw new Exception($"No device has been registered as [{type.Name}]");

            return this[type];
        }

        /// <inheritdoc/>
        public bool ExistsKeyed(Type deviceType, object key)
        {
            return  ContainsKey(deviceType) &&
                this[deviceType]
                    .Any(x => key.IsEqualTo(x.Key));
        }

        /// <inheritdoc/>
        public bool Exists(Type deviceType)
        {
            return ContainsKey(deviceType);
        }
    }
}
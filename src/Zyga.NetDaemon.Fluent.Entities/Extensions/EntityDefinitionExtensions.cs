using NetDaemon.Fluent.Entities.Maps.Roots;
using NetDaemon.Fluent.Entities.Model;
using NetDaemon.Fluent.Entities.Model.EntityCreation;
using System.Dynamic;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Extensions
{
    /// <summary>Extension methods on <see cref="EntityDefinition"/>.</summary>
    internal static class EntityDefinitionExtensions
    {
        /// <summary>
        /// Builds the MQTT discovery "additional config" payload for an entity: every public, readable
        /// property on <paramref name="definition"/>'s concrete type marked with <see cref="Model.EntityCreation.AdditionalConfigAttribute"/>
        /// and holding a non-null value is added under that attribute's configured name, plus a <c>"device"</c>
        /// entry built from <paramref name="deviceDefinition"/> when one is supplied.
        /// </summary>
        /// <param name="definition">The entity definition to read additional-config properties from.</param>
        /// <param name="deviceDefinition">The owning device, used to populate the <c>"device"</c> entry.</param>
        public static ExpandoObject BuildAdditionalConfig(this EntityDefinition definition, DeviceDefinition? deviceDefinition = null)
        {
            if (definition is null) throw new ArgumentNullException(nameof(definition));

            IDictionary<string, object?> result = new ExpandoObject();
            var type = definition.GetType();

            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!prop.CanRead) continue;

                var attr = prop.GetCustomAttribute<AdditionalConfigAttribute>(inherit: true);
                if (attr is null) continue;

                var value = prop.GetValue(definition);
                if (value is null) continue;

                
                result[attr.Name] = value;
            }

            if (deviceDefinition != null)
                result["device"] = new DeviceConfig(deviceDefinition);

            return (ExpandoObject)result;
        }
    }
}
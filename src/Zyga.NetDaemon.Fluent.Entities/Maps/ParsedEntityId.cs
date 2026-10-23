using System.Text.RegularExpressions;

namespace NetDaemon.Fluent.Entities.Maps
{
    /// <summary>
    /// Represents a validated, composed Home Assistant entity id of the form <c>{prefix}.{deviceId}_{name}</c>,
    /// built from a short or fully-qualified entity id string together with the owning device's id.
    /// </summary>
    public class ParsedEntityId
    {
        /// <summary>
        /// Parses and validates an entity id. <paramref name="entityId"/> may be a short name (e.g. <c>"max_up"</c>)
        /// or a fully-qualified one matching <paramref name="prefix"/> (e.g. <c>"binary_sensor.max_up"</c>); either
        /// way the device id prefix is stripped if already present, and the result is validated against
        /// <c>^[a-z0-9_]+\.[a-z0-9_]+$</c>.
        /// </summary>
        /// <param name="entityId">The short or fully-qualified entity id.</param>
        /// <param name="prefix">The expected HA domain prefix (e.g. <c>"sensor"</c>).</param>
        /// <param name="deviceId">The owning device's identifier.</param>
        /// <exception cref="InvalidDataException">Thrown when <paramref name="entityId"/> is blank, contains more than one <c>.</c>, has a prefix that does not match <paramref name="prefix"/>, or fails the final validation pattern.</exception>
        public static ParsedEntityId Parse(string entityId, string prefix, string deviceId)
        {
            if (string.IsNullOrWhiteSpace(entityId))
                throw new InvalidDataException("EntityId has to be valid string");

            string parsedEntityName = entityId;

            var parts = entityId.Split(".");

            if (parts.Length > 2)
                throw new InvalidDataException($"There should be only one [.] in entity name [{entityId}]");

            if (parts.Length == 2)
            {
                if (parts[0] != prefix)
                    throw new InvalidDataException($"Provided prefix [{parts[0]}] is different than expected [{prefix}]");

                parsedEntityName = parts[1];
            }

            var deviceIdPrefix = $"{deviceId}_";

            if (parsedEntityName.StartsWith(deviceIdPrefix))
                parsedEntityName = parsedEntityName
                    .Replace(deviceIdPrefix, string.Empty);
            
            var entityIdObject = new ParsedEntityId()
            {
                Prefix = prefix,
                DeviceId = deviceId,
                EntityName = parsedEntityName
            };

            var pattern = @"^[a-z0-9_]+\.[a-z0-9_]+$";
            if (!Regex.IsMatch(entityIdObject.ToString(), pattern))
                throw new InvalidDataException($"Incorrect entityId [{entityIdObject}]");

            return entityIdObject;
        }


        /// <summary>Gets or sets the HA domain prefix (e.g. <c>"sensor"</c>).</summary>
        public required string Prefix { get; set; }

        /// <summary>Gets or sets the owning device's identifier.</summary>
        public required string DeviceId { get; set; }

        /// <summary>Gets or sets the short entity name, with the device id prefix already stripped.</summary>
        public required string EntityName { get; set; }

        /// <summary>Returns the fully composed entity id, <c>{Prefix}.{DeviceId}_{EntityName}</c>.</summary>
        public override string ToString()
        {
            return $"{Prefix}.{DeviceId}_{EntityName}";
        }
    }
}
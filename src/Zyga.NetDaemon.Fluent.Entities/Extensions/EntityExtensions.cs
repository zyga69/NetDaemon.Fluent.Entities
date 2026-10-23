using NetDaemon.HassModel.Entities;

namespace NetDaemon.Fluent.Entities.Extensions
{
    /// <summary>Extension methods on <see cref="NetDaemon.HassModel.Entities.Entity"/>.</summary>
    public static class EntityExtensions
    {
        /// <summary>Returns the entity's on/off state as a <see cref="bool"/>, or <c>null</c> when it is neither on nor off.</summary>
        /// <param name="entity">The entity to read the state of.</param>
        public static bool? GetBinarySensorState(this NetDaemon.HassModel.Entities.Entity entity)
        {
            if (entity.IsOn())
                return true;

            if (entity.IsOff())
                return false;

            return null;
        }
    }
}
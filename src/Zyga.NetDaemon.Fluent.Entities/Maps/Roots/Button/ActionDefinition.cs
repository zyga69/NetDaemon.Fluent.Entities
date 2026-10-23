using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Button
{
    /// <summary>
    /// Describes a HA <c>button</c> entity backed by a parameterless action method on the device model.
    /// Pressing the button in HA invokes <see cref="Method"/> on the device instance.
    /// </summary>
    /// <param name="shortEntityId">The short (unprefixed) entity id, e.g. <c>gate</c>.</param>
    /// <param name="prefix">The HA domain prefix, typically <c>button</c>.</param>
    /// <param name="deviceId">The owning device's identifier.</param>
    /// <param name="method">The device model method invoked when the button is pressed.</param>
    /// <param name="attributes">Optional friendly name/icon attributes.</param>
    public class ActionDefinition(string shortEntityId, string prefix, string deviceId, MethodInfo method, EntityAttributes? attributes = null)
          : EntityDefinition(shortEntityId, prefix, deviceId, attributes)
    {
        /// <summary>Gets the device model method invoked when the button is pressed.</summary>
        public MethodInfo Method { get; private set; } = method;

        /// <summary>Returns a diagnostic string representation of this definition.</summary>
        public override string ToString()
        {
            return $"[Button|{Method.Name}]: {base.ToString()}";
        }
    }
}
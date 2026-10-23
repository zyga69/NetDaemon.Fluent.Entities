using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Button
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsButton()</c>. Creates the <see cref="ActionDefinition"/>
    /// for the mapped method and registers it with the owning device.
    /// </summary>
    public class ButtonMapRoot
    {
        /// <summary>Creates and registers the button's <see cref="ActionDefinition"/> on the device.</summary>
        /// <param name="method">The device model method invoked when the button is pressed.</param>
        /// <param name="entityId">The HA entity ID (short or fully qualified with the <c>button</c> prefix).</param>
        /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public ButtonMapRoot(MethodInfo method, string entityId, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
        {
            deviceDefinition.Entities.Add(new ActionDefinition(entityId, "button", deviceDefinition.Identifier, method, attributes));
        }
    }
}
using NetDaemon.Fluent.Entities.Model;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Maps.Roots.Switch
{
    /// <summary>
    /// Terminal fluent root produced by <c>AsSwitch()</c>. Creates the <see cref="SwitchDefinition"/> for the
    /// mapped property and registers it with the owning device. Chain <see cref="InitiallyOn"/> or
    /// <see cref="InitiallyOff"/> to set the switch's state when it is first created.
    /// </summary>
    public class SwitchMapRoot
    {
        private readonly SwitchDefinition _switchDefinition;

        /// <summary>Creates and registers the switch's <see cref="SwitchDefinition"/> on the device.</summary>
        /// <param name="propertyInfo">The device model property associated with the switch.</param>
        /// <param name="entityId">The HA entity ID (short or fully qualified with the <c>switch</c> prefix).</param>
        /// <param name="deviceDefinition">The owning device definition to register the entity with.</param>
        /// <param name="attributes">Optional friendly name/icon attributes.</param>
        public SwitchMapRoot(PropertyInfo propertyInfo, string entityId, DeviceDefinition deviceDefinition, EntityAttributes? attributes = null)
        {
            _switchDefinition = new SwitchDefinition(entityId, "switch", deviceDefinition.Identifier, propertyInfo, attributes);
            deviceDefinition.Entities.Add(_switchDefinition);
        }

        /// <summary>Sets the switch to be turned on when it is first created.</summary>
        public void InitiallyOn() => _switchDefinition.InitialState = true;

        /// <summary>Sets the switch to be turned off when it is first created. This is the default.</summary>
        public void InitiallyOff() => _switchDefinition.InitialState = false;
    }
}
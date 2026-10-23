using Material.Icons;
using System.Text.RegularExpressions;

namespace NetDaemon.Fluent.Entities.Extensions
{
    /// <summary>Extension methods on <see cref="MaterialIconKind"/>.</summary>
    internal static class MaterialIconKindExtensions
    {
        /// <summary>Renders a <see cref="MaterialIconKind"/> value to the <c>mdi:kebab-case-name</c> string Home Assistant expects for an icon.</summary>
        /// <param name="icon">The icon to render.</param>
        public static string ToMdi(this MaterialIconKind icon)
        {
            return "mdi:" +
                Regex.Replace(icon.ToString(), "([a-z0-9])([A-Z])", "$1-$2")
                     .ToLowerInvariant();
        }
    }
}
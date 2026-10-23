using System.ComponentModel;
using System.Reflection;

namespace NetDaemon.Fluent.Entities.Common.Extensions
{
    internal static class EnumExtensions
    {
        public static T? GetAttribute<T>(this Enum @enum) where T : Attribute
        {
            var type = @enum.GetType();
            var name = Enum.GetName(@enum.GetType(), @enum);

            if (name == null)
                return default;

            var field = type.GetField(name);
            var attribute = field?.GetCustomAttribute<T>();
            return attribute;
        }

        public static string GetDescription(this Enum @enum)
        {
            return @enum.GetAttribute<DescriptionAttribute>()?.Description ?? @enum.ToString();
        }
    }
}

using System.Reflection;

namespace NetDaemon.Fluent.Entities.Common.Extensions
{
    /// <summary>Extension methods on <see cref="ParameterInfo"/>.</summary>
    public static class ParameterInfoExtensions
    {
        /// <summary>Returns whether the parameter's declared type is nullable (a nullable reference or value type).</summary>
        /// <param name="parameter">The parameter to inspect.</param>
        public static bool IsNullable(this ParameterInfo parameter)
        {
            var context = new NullabilityInfoContext();
            var nullability = context.Create(parameter);

            return nullability.ReadState == NullabilityState.Nullable;
        }
    }
}

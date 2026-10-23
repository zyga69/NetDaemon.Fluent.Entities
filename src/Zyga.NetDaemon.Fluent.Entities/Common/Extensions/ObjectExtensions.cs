namespace NetDaemon.Fluent.Entities.Common.Extensions
{
    /// <summary>General-purpose extension methods on <see cref="object"/>.</summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Returns whether <paramref name="b"/> is non-null, of the exact same runtime type as <paramref name="a"/>,
        /// and <see cref="object.Equals(object?)"/> to it.
        /// </summary>
        /// <param name="a">The value to compare from.</param>
        /// <param name="b">The value to compare against.</param>
        public static bool IsEqualTo(this object a, object? b)
        {
            if (b == null)
                return false;

            if (a.GetType() != b.GetType())
                return false;

            return a.Equals(b);
        }
    }
}

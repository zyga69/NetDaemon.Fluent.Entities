namespace NetDaemon.Fluent.Entities.DependencyInjection
{
    /// <summary>
    /// Marker interface for types that a consuming host's DI scan should register with scoped lifetime
    /// (as-self and as-implemented-interfaces). See the library's README "Wiring the library into a host" section.
    /// </summary>
    public interface IScopedService
    {
    }
}

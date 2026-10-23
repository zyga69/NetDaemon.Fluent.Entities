[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("NetDaemon.Fluent.Entities.Tests")]
// NSubstitute (via Castle DynamicProxy) generates mock types in a dynamic assembly named
// "DynamicProxyGenAssembly2" by convention. Mocking an internal interface/class requires that
// dynamic assembly to see internals too, not just the real NetDaemon.Fluent.Entities.Tests assembly.
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DynamicProxyGenAssembly2")]

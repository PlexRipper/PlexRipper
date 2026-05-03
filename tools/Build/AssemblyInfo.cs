using System.Runtime.CompilerServices;

// Needed for Moq/Castle DynamicProxy in unit tests: Build contains internal interfaces
// (e.g. IDesktopCommandRunner). Without this, tests fail with "type is not accessible"
// when AutoMock tries to create proxies for those internals.
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

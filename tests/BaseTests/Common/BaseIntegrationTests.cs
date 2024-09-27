using Data.Contracts;
using Logging.Interface;
using Serilog.Events;

namespace PlexRipper.BaseTests;

[Collection("Sequential")]
public class BaseIntegrationTests : IAsyncLifetime
{
    protected BaseContainer Container = null!;
    private readonly ILog _log;

    protected BaseIntegrationTests(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Verbose)
    {
        System.Environment.SetEnvironmentVariable("LOG_LEVEL", logLevel.ToString().ToUpper());

        // Ensure that the test output helper is set first
        LogConfig.SetTestOutputHelper(output);

        LogManager.SetupLogging(logLevel);
        _log = LogManager.CreateLogInstance(typeof(BaseIntegrationTests));

        BogusExtensions.Setup();
    }

    protected async Task CreateContainer(Action<UnitTestDataConfig>? options = null)
    {
        Container = await BaseContainer.Create(_log, options);
    }

    protected IPlexRipperDbContext DbContext => Container.Resolve<IPlexRipperDbContext>();

    protected string DatabaseName => Container.DatabaseName;

    public Task InitializeAsync()
    {
        _log.InformationLine("Initialize Integration Test");
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _log.Warning("Integration Test with DatabaseName: \"{DatabaseName}\" has ended, Disposing!", DatabaseName);

        await Container.Boot.StopAsync(CancellationToken.None);
        Container.Dispose();

        _log.FatalLine("Container disposed");
    }
}

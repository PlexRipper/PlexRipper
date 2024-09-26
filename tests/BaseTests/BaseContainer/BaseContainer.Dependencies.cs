using Application.Contracts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using PlexRipper.Data;
using PlexRipper.WebAPI;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

public partial class BaseContainer : IDisposable
{
    private readonly PlexRipperWebApplicationFactory _factory;

    private readonly ILog _log;

    private readonly ILifetimeScope _lifeTimeScope;

    public string DatabaseName => _factory.MemoryDbName;

    /// <summary>
    /// Creates an Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(ILog log, string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        _log = log;

        _log.Information("Setting up BaseContainer with database: {MemoryDbName}", memoryDbName);

        _factory = new PlexRipperWebApplicationFactory(memoryDbName, options);
        ApiClient = _factory.CreateDefaultClient();

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope(memoryDbName);
    }

    public static async Task<BaseContainer> Create(ILog log, Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);
        EnvironmentExtensions.SetIntegrationTestMode(true);

        var memoryDbName = MockDatabase.GetMemoryDatabaseName();

        log.Information("Initialized integration test with database name: {DatabaseName}", memoryDbName);

        var container = new BaseContainer(log, memoryDbName, options);

        await MockDatabase
            .GetMemoryDbContext(memoryDbName)
            .Setup(config.Seed, config.DatabaseOptions, container.PlexMockServers);

        if (config.DownloadSpeedLimitInKib > 0)
            await container.SetDownloadSpeedLimit(options);

        return container;
    }

    public HttpClient ApiClient { get; }

    public IFileSystem FileSystem => Resolve<IFileSystem>();

    public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

    public IPlexApiService GetPlexApiService => Resolve<IPlexApiService>();

    public IMediator Mediator => Resolve<IMediator>();

    public IPathProvider PathProvider => Resolve<IPathProvider>();

    public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

    public IFileMergeScheduler FileMergeScheduler => Resolve<IFileMergeScheduler>();

    public MockSignalRService MockSignalRService => (MockSignalRService)Resolve<ISignalRService>();

    public IBoot Boot => Resolve<IBoot>();

    public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();
}

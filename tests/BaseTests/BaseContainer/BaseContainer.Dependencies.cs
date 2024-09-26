using Application.Contracts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Data.Contracts;
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

    private static ILog _log;

    private readonly ILifetimeScope _lifeTimeScope;

    /// <summary>
    /// Creates a Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        _factory = new PlexRipperWebApplicationFactory(memoryDbName, options);
        ApiClient = _factory.CreateDefaultClient();

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope(memoryDbName);
    }

    public static async Task<BaseContainer> Create(ILog log, Action<UnitTestDataConfig>? options = null)
    {
        _log = log;
        var config = UnitTestDataConfig.FromOptions(options);
        EnvironmentExtensions.SetIntegrationTestMode(true);

        var memoryDbName = MockDatabase.GetMemoryDatabaseName();
        _log.Information("Setting up BaseContainer with database: {MemoryDbName}", memoryDbName);

        var container = new BaseContainer(memoryDbName, options);

        // TODO verifiy seed
        await MockDatabase.GetMemoryDbContext(memoryDbName).Setup(0, config.DatabaseOptions, container.PlexMockServers);

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

    public IPlexRipperDbContext IPlexRipperDbContext => Resolve<IPlexRipperDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

    public IFileMergeScheduler FileMergeScheduler => Resolve<IFileMergeScheduler>();

    public MockSignalRService MockSignalRService => (MockSignalRService)Resolve<ISignalRService>();

    public TestLoggingClass TestLoggingClass => Resolve<TestLoggingClass>();

    public IBoot Boot => Resolve<IBoot>();

    public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();
}

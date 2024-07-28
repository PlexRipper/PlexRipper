#region

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

#endregion

namespace PlexRipper.BaseTests;

public partial class BaseContainer : IDisposable
{
    #region Fields

    private readonly PlexRipperWebApplicationFactory _factory;

    private static ILog _log;

    private readonly ILifetimeScope _lifeTimeScope;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(
        string memoryDbName,
        Action<UnitTestDataConfig>? options = null,
        MockPlexApi mockPlexApi = null
    )
    {
        _factory = new PlexRipperWebApplicationFactory(memoryDbName, options, mockPlexApi);
        ApiClient = _factory.CreateDefaultClient();

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope(memoryDbName);
    }

    public static async Task<BaseContainer> Create(
        ILog log,
        string memoryDbName,
        Action<UnitTestDataConfig>? options = null,
        MockPlexApi mockPlexApi = null
    )
    {
        _log = log;
        var config = UnitTestDataConfig.FromOptions(options);

        _log.Information("Setting up BaseContainer with database: {MemoryDbName}", memoryDbName);

        EnvironmentExtensions.SetIntegrationTestMode(true);

        var container = new BaseContainer(memoryDbName, options, mockPlexApi);

        if (config.DownloadSpeedLimitInKib > 0)
            await container.SetDownloadSpeedLimit(options);

        return container;
    }

    #endregion

    #region Properties

    public HttpClient ApiClient { get; }

    #region Autofac Resolve

    public IFileSystem FileSystem => Resolve<IFileSystem>();

    public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

    public IPlexApiService GetPlexApiService => Resolve<IPlexApiService>();

    public IMediator Mediator => Resolve<IMediator>();

    public IPathProvider PathProvider => Resolve<IPathProvider>();

    public ITestStreamTracker TestStreamTracker => Resolve<ITestStreamTracker>();

    public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

    public IPlexRipperDbContext IPlexRipperDbContext => Resolve<IPlexRipperDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

    public IFileMergeScheduler FileMergeScheduler => Resolve<IFileMergeScheduler>();

    public MockSignalRService MockSignalRService => (MockSignalRService)Resolve<ISignalRService>();

    public TestLoggingClass TestLoggingClass => Resolve<TestLoggingClass>();

    public IBoot Boot => Resolve<IBoot>();

    #region Settings

    public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();

    public IConfigManager ConfigManager => Resolve<IConfigManager>();

    #endregion

    #endregion

    #endregion

    #region Public Methods

    private T Resolve<T>() => _lifeTimeScope.Resolve<T>();

    #endregion

    public void Dispose()
    {
        _log.WarningLine("Disposing Container");
        PlexRipperDbContext.Database.EnsureDeleted();
        _lifeTimeScope.Dispose();
        _factory?.Dispose();
        ApiClient?.Dispose();
    }
}

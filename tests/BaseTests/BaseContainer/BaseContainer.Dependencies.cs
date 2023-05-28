#region

using Application.Contracts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using BackgroundServices.Contracts;
using DownloadManager.Contracts;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc.Testing;
using PlexApi.Contracts;
using PlexRipper.Data;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI;
using Settings.Contracts;
using WebAPI.Contracts;

#endregion

namespace PlexRipper.BaseTests;

public partial class BaseContainer : IDisposable
{
    #region Fields

    private readonly WebApplicationFactory<Startup> _factory;

    private static ILog _log;

    private readonly ILifetimeScope _lifeTimeScope;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(string memoryDbName, Action<UnitTestDataConfig> options = null, MockPlexApi mockPlexApi = null)
    {
        _factory = new PlexRipperWebApplicationFactory<Startup>(memoryDbName, options, mockPlexApi);
        ApiClient = _factory.CreateDefaultClient();

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope(memoryDbName);
    }

    public static async Task<BaseContainer> Create(
        ILog log,
        string memoryDbName,
        Action<UnitTestDataConfig> options = null,
        MockPlexApi mockPlexApi = null)
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

    public IDownloadCommands GetDownloadCommands => Resolve<IDownloadCommands>();

    public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

    public IDownloadTaskFactory GetDownloadTaskFactory => Resolve<IDownloadTaskFactory>();

    public IDownloadTaskValidator GetDownloadTaskValidator => Resolve<IDownloadTaskValidator>();

    public IFolderPathService GetFolderPathService => Resolve<IFolderPathService>();

    public IPlexAccountService GetPlexAccountService => Resolve<IPlexAccountService>();

    public IPlexApiService GetPlexApiService => Resolve<IPlexApiService>();

    public IPlexLibraryService GetPlexLibraryService => Resolve<IPlexLibraryService>();

    public IPlexServerService GetPlexServerService => Resolve<IPlexServerService>();

    public IMediator Mediator => Resolve<IMediator>();

    public IPathProvider PathProvider => Resolve<IPathProvider>();

    public ITestStreamTracker TestStreamTracker => Resolve<ITestStreamTracker>();

    public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

    public IFileMergeScheduler FileMergeScheduler => Resolve<IFileMergeScheduler>();
    public MockSignalRService MockSignalRService => (MockSignalRService)Resolve<ISignalRService>();

    public TestLoggingClass TestLoggingClass => Resolve<TestLoggingClass>();

    public IMapper Mapper => Resolve<IMapper>();

    public IBoot Boot => Resolve<IBoot>();

    #region Settings

    public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();

    public IConfigManager ConfigManager => Resolve<IConfigManager>();

    #endregion

    #endregion

    #endregion

    #region Public Methods

    private T Resolve<T>()
    {
        return _lifeTimeScope.Resolve<T>();
    }

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
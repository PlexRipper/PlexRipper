#region

using Application.Contracts;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using BackgroundServices.Contracts;
using DownloadManager.Contracts;
using Environment;
using FileSystem.Contracts;
using HttpClient.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PlexApi.Contracts;
using PlexRipper.Data;
using PlexRipper.DownloadManager;
using PlexRipper.WebAPI;
using Settings.Contracts;

#endregion

namespace PlexRipper.BaseTests;

public partial class BaseContainer : IDisposable
{
    #region Fields

    private readonly WebApplicationFactory<Startup> _factory;

    private readonly IServiceScope _serviceScope;

    private readonly IServiceProvider _services;

    private static ILog _log;

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
        _services = _factory.Services;
        _serviceScope = _factory.Services.CreateScope();
    }

    public static async Task<BaseContainer> Create(
        ILog log,
        string memoryDbName,
        int seed = 0,
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

    public System.Net.Http.HttpClient ApiClient { get; }

    #region Autofac Resolve

    public IFileSystem FileSystem => Resolve<IFileSystem>();

    public IDownloadCommands GetDownloadCommands => Resolve<IDownloadCommands>();

    public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

    public IDownloadTaskFactory GetDownloadTaskFactory => Resolve<IDownloadTaskFactory>();

    public IDownloadTaskValidator GetDownloadTaskValidator => Resolve<IDownloadTaskValidator>();

    public IFolderPathService GetFolderPathService => Resolve<IFolderPathService>();

    public IPlexAccountService GetPlexAccountService => Resolve<IPlexAccountService>();

    public IPlexApiService GetPlexApiService => Resolve<IPlexApiService>();

    public IPlexDownloadService GetPlexDownloadService => Resolve<IPlexDownloadService>();

    public IPlexLibraryService GetPlexLibraryService => Resolve<IPlexLibraryService>();

    public IPlexServerService GetPlexServerService => Resolve<IPlexServerService>();

    public IMediator Mediator => Resolve<IMediator>();

    public IPathProvider PathProvider => Resolve<IPathProvider>();

    public ITestStreamTracker TestStreamTracker => Resolve<ITestStreamTracker>();

    public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

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
        return _services.GetRequiredService<T>();
    }

    #endregion

    public void Dispose()
    {
        _log.WarningLine("Disposing Container");
        PlexRipperDbContext.Database.EnsureDeleted();
        _services.GetAutofacRoot().Dispose();
        _factory?.Dispose();
        ApiClient?.Dispose();
    }
}
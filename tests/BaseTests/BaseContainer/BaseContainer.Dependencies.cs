#region

using Application.Contracts;
using Autofac.Extensions.DependencyInjection;
using AutoMapper;
using DownloadManager.Contracts;
using Environment;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PlexRipper.Application;
using PlexRipper.Data;
using PlexRipper.DownloadManager;
using PlexRipper.HttpClient;
using PlexRipper.WebAPI;

#endregion

namespace PlexRipper.BaseTests;

public partial class BaseContainer : IDisposable
{
    #region Fields

    private readonly WebApplicationFactory<Startup> _factory;

    private readonly IServiceScope _serviceScope;

    private readonly IServiceProvider _services;

    #endregion

    #region Constructor

    /// <summary>
    /// Creates a Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(string memoryDbName, Action<UnitTestDataConfig> options = null, MockPlexApi mockPlexApi = null)
    {
        _factory = new PlexRipperWebApplicationFactory<Startup>(memoryDbName, options, mockPlexApi);
        ApiClient = _factory.CreateClient();

        // Create a separate scope as not to interfere with tests running in parallel
        _services = _factory.Services;
        _serviceScope = _factory.Services.CreateScope();
    }

    public static async Task<BaseContainer> Create(string memoryDbName, int seed = 0, Action<UnitTestDataConfig> options = null, MockPlexApi mockPlexApi = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        Log.Information($"Setting up BaseContainer with database: {memoryDbName}");

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

    public IPlexRipperHttpClient GetPlexRipperHttpClient => Resolve<IPlexRipperHttpClient>();

    public IPlexServerService GetPlexServerService => Resolve<IPlexServerService>();

    public IMediator Mediator => Resolve<IMediator>();

    public IPathProvider PathProvider => Resolve<IPathProvider>();

    public ITestStreamTracker TestStreamTracker => Resolve<ITestStreamTracker>();

    public PlexRipperDbContext PlexRipperDbContext => Resolve<PlexRipperDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

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
        Log.Warning("Disposing Container");
        PlexRipperDbContext.Database.EnsureDeleted();
        _services.GetAutofacRoot().Dispose();
        _factory?.Dispose();
        ApiClient?.Dispose();
    }
}
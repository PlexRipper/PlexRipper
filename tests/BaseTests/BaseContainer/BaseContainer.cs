using Application.Contracts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;
using PlexRipper.Data;
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

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope();
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

    public HttpClient ApiClient => _factory.CreateDefaultClient();

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

    public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();

    public IPlexRipperDbContext DbContext => Resolve<IPlexRipperDbContext>();

    public async Task SetDownloadSpeedLimit(Action<UnitTestDataConfig>? options = null)
    {
        var config = new UnitTestDataConfig();
        options?.Invoke(config);

        var plexServers = await PlexRipperDbContext.PlexServers.ToListAsync();
        foreach (var plexServer in plexServers)
            GetServerSettings.SetDownloadSpeedLimit(plexServer.MachineIdentifier, config.DownloadSpeedLimitInKib);
    }

    public T Resolve<T>()
        where T : notnull => _lifeTimeScope.Resolve<T>();

    public List<PlexMockServer> PlexMockServers => _factory.PlexMockServers;

    public void Dispose()
    {
        _log.Warning(
            "Integration Test with DatabaseName: \"{DatabaseName}\" has ended, Disposing!",
            DbContext.DatabaseName
        );

        try
        {
            // Ensure the database is deleted
            PlexRipperDbContext.Database.EnsureDeleted();
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(
                    "Failed to delete database: {DatabaseName}, Error: {ExceptionMessage}",
                    DatabaseName,
                    ex.Message
                );
        }

        // Dispose of the HttpClient and factory before the lifetime scope
        ApiClient.Dispose();
        _factory.Dispose();

        // Dispose of the lifetime scope as the last step
        _lifeTimeScope.Dispose();

        _log.FatalLine("Container disposed");
    }
}

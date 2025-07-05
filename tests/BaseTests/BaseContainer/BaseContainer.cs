using System.Net.Http.Headers;
using Application.Contracts;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Data.Contracts;
using Environment;
using FileSystem.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

public class BaseContainer : IDisposable
{
    private readonly PlexRipperWebApplicationFactory _factory;

    private readonly ILog _log;

    private readonly ILifetimeScope _lifeTimeScope;

    public string DatabaseName => _factory.MemoryDbName;

    /// <summary>
    /// Creates an Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(ILog log, Seed seed, string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        _log = log;

        _log.Information("Setting up BaseContainer with database: {MemoryDbName}", memoryDbName);

        _factory = new PlexRipperWebApplicationFactory(seed, memoryDbName, options);

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope();
    }

    public static async Task<BaseContainer> Create(ILog log, Seed seed, Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        EnvironmentExtensions.SetIntegrationTestMode(true);

        var memoryDbName = MockDatabase.GetMemoryDatabaseName();

        log.Information("Initialized integration test with database name: {DatabaseName}", memoryDbName);

        await MockDatabase.GetMemoryDbContext(memoryDbName).Setup(seed, config.DatabaseOptions);

        var container = new BaseContainer(log, seed, memoryDbName, options);

        if (config.DownloadSpeedLimitInKib > 0)
            await container.SetDownloadSpeedLimit(options);

        return container;
    }

    public HttpClient GetApiClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme: "TestScheme");
        return client;
    }

    public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

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

    public void Dispose()
    {
        _log.Warning(
            "Integration Test with DatabaseName: \"{DatabaseName}\" has ended, Disposing!",
            DbContext.DatabaseName
        );

        // Wait for any pending async operations to complete before disposing
        // This prevents ObjectDisposedException when FastEndpoints command handlers
        // are still executing in background threads during parallel test execution
        Task.Delay(TimeSpan.FromSeconds(1)).Wait();

        Result.Try(
            () => PlexRipperDbContext.Database.EnsureDeleted(),
            ex =>
                _log.Here()
                    .Error(
                        "Failed to delete database: {DatabaseName}, Error: {ExceptionMessage}",
                        DatabaseName,
                        ex.Message
                    )
                    .ToResult()
                    .Errors.First()
        );

        _factory.Dispose();

        // Dispose of the lifetime scope as the last step
        _lifeTimeScope.Dispose();

        _log.FatalLine("Container disposed");
    }
}

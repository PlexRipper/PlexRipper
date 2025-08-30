using System.Net.Http.Headers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.FileSystem.Contracts;
using Reaparr.Logging;
using Reaparr.Settings.Contracts;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.BaseTests;

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
        var dbName = DatabaseName;
        _log.Warning("Integration Test with DatabaseName: \"{DatabaseName}\" has ended, Disposing!", dbName);

        // Wait for any pending async operations to complete before disposing
        // This prevents ObjectDisposedException when FastEndpoints command handlers
        // are still executing in background threads during parallel test execution
        _log.Information("Waiting for async operations to complete before disposing container {DatabaseName}", dbName);

        try
        {
            // Use a more robust delay mechanism
            var delay = Task.Delay(TimeSpan.FromSeconds(3));
            delay.Wait();
            _log.Information("Async operations wait completed for container {DatabaseName}", dbName);
        }
        catch (Exception ex)
        {
            _log.Error("Error during async operations wait: {Error}", ex.Message);
        }

        try
        {
            // Ensure the database is deleted
            PlexRipperDbContext.Database.EnsureDeleted();
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error("Failed to delete database: {DatabaseName}, Error: {ExceptionMessage}", dbName, ex.Message);
        }

        _log.Information("Disposing factory for container {DatabaseName}", dbName);
        _factory.Dispose();

        // Dispose of the lifetime scope as the last step
        _log.Information("Disposing lifetime scope for container {DatabaseName}", dbName);
        _lifeTimeScope.Dispose();

        _log.FatalLine("Container disposed");
    }
}

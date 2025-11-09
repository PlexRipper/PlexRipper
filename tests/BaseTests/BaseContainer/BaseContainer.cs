using System.Net.Http.Headers;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using FastEndpoints;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data;
using Reaparr.Data.Contracts;
using Reaparr.Environment;
using Reaparr.FileSystem.Contracts;
using Reaparr.PublicAPI;
using Reaparr.Settings.Contracts;

namespace Reaparr.BaseTests;

public class BaseContainer : IDisposable
{
    private readonly ReaparrWebApplicationFactory _factory;

    private readonly ILogger _log;

    private readonly ILifetimeScope _lifeTimeScope;

    public string DatabaseName => _factory.MemoryDbName;

    /// <summary>
    /// Creates an Autofac container and sets up a test database.
    /// </summary>
    private BaseContainer(ILogger log, Seed seed, string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        _log = log.ForContext<BaseContainer>();

        _log.Here().Information("Setting up BaseContainer with database: {MemoryDbName}", memoryDbName);

        _factory = new ReaparrWebApplicationFactory(seed, memoryDbName, options);

        // Create a separate scope as not to interfere with tests running in parallel
        _lifeTimeScope = _factory.Services.GetAutofacRoot().BeginLifetimeScope();
    }

    public static async Task<BaseContainer> Create(ILogger log, Seed seed, Action<UnitTestDataConfig>? options = null)
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

    public async Task SignInDownloadClient(HttpClient client)
    {
        var integrationsSettings = Resolve<IIntegrationsSettings>();
        var loginRequest = new DownloadClientLoginEndpointRequest
        {
            Username = integrationsSettings.DownloadClientUsername,
            Password = integrationsSettings.DownloadClientPassword,
        };

        var response = await client.POSTAsync<DownloadClientLoginEndpoint, DownloadClientLoginEndpointRequest>(
            loginRequest
        );

        response.IsSuccessStatusCode.ShouldBeTrue("Failed to login the download client test HttpClient.");
    }

    public IDownloadQueue GetDownloadQueue => Resolve<IDownloadQueue>();

    public IPathProvider PathProvider => Resolve<IPathProvider>();

    public ReaparrDbContext ReaparrDbContext => Resolve<ReaparrDbContext>();

    public ISchedulerService SchedulerService => Resolve<ISchedulerService>();

    public IDownloadTaskScheduler DownloadTaskScheduler => Resolve<IDownloadTaskScheduler>();

    public IMoveDownloadFileScheduler MoveDownloadFileScheduler => Resolve<IMoveDownloadFileScheduler>();

    public MockSignalRService MockSignalRService => (MockSignalRService)Resolve<ISignalRService>();

    public IServerSettingsModule GetServerSettings => Resolve<IServerSettingsModule>();

    public IReaparrDbContext DbContext => Resolve<IReaparrDbContext>();

    public async Task SetDownloadSpeedLimit(Action<UnitTestDataConfig>? options = null)
    {
        var config = new UnitTestDataConfig();
        options?.Invoke(config);

        var plexServers = await ReaparrDbContext.PlexServers.ToListAsync();
        foreach (var plexServer in plexServers)
            GetServerSettings.SetDownloadSpeedLimit(plexServer.MachineIdentifier, config.DownloadSpeedLimitInKib);
    }

    public T Resolve<T>()
        where T : notnull => _lifeTimeScope.Resolve<T>();

    public void Dispose()
    {
        var dbName = DatabaseName;
        _log.Here().Warning("Integration Test with DatabaseName: \"{DatabaseName}\" has ended, Disposing!", dbName);

        // Wait for any pending async operations to complete before disposing
        // This prevents ObjectDisposedException when FastEndpoints command handlers
        // are still executing in background threads during parallel test execution
        _log.Here()
            .Information("Waiting for async operations to complete before disposing container {DatabaseName}", dbName);

        try
        {
            // Use a more robust delay mechanism
            var delay = Task.Delay(TimeSpan.FromSeconds(3));
            delay.Wait();
            _log.Here().Information("Async operations wait completed for container {DatabaseName}", dbName);
        }
        catch (Exception ex)
        {
            _log.Here().Error("Error during async operations wait: {Error}", ex.Message);
        }

        try
        {
            // Ensure the database is deleted
            ReaparrDbContext.Database.EnsureDeleted();
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error("Failed to delete database: {DatabaseName}, Error: {ExceptionMessage}", dbName, ex.Message);
        }

        _log.Here().Information("Disposing factory for container {DatabaseName}", dbName);
        _factory.Dispose();

        // Dispose of the lifetime scope as the last step
        _log.Here().Information("Disposing lifetime scope for container {DatabaseName}", dbName);
        _lifeTimeScope.Dispose();

        _log.Here().Fatal("Container disposed");
    }
}

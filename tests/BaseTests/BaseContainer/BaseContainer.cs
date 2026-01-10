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
        var settings = Resolve<IIntegrationsSettings>();

        // Needs to be sent as application/x-www-form-urlencoded content
        // which is not properly supported by FastEndpoints test client
        var content = new FormUrlEncodedContent([
            new KeyValuePair<string, string>("username", settings.DownloadClientUsername),
            new KeyValuePair<string, string>("password", settings.DownloadClientPassword),
        ]);

        var response = await client.PostAsync(PublicApiRoutes.DownloadClient + "/auth/login", content);

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

    /// <summary>
    /// Waits for a download task to reach one of the specified statuses by monitoring SignalR progress updates.
    /// This is event-driven rather than polling-based, making tests more reliable.
    /// </summary>
    /// <param name="downloadTaskId">The GUID of the download task to monitor.</param>
    /// <param name="targetStatuses">The statuses to wait for.</param>
    /// <param name="timeout">Maximum time to wait.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The final download progress DTO when the target status is reached.</returns>
    public async Task<DownloadProgressDTO?> WaitForDownloadStatusAsync(
        Guid downloadTaskId,
        DownloadStatus[] targetStatuses,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        timeout ??= TimeSpan.FromSeconds(30);
        var deadline = DateTime.UtcNow.Add(timeout.Value);
        var pollInterval = TimeSpan.FromMilliseconds(500);

        while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
        {
            // Check SignalR events (non-blocking with short timeout)
            try
            {
                if (MockSignalRService.ServerDownloadProgressList.TryTake(out var progress, 100, cancellationToken))
                {
                    var matchingDownload = FindDownloadById(progress.Downloads, downloadTaskId);
                    if (matchingDownload != null && targetStatuses.Contains(matchingDownload.Status))
                    {
                        _log.Here()
                            .Debug(
                                "Download task {DownloadTaskId} reached status {Status} via SignalR",
                                downloadTaskId,
                                matchingDownload.Status
                            );
                        return matchingDownload;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }

            // Poll database as fallback
            var dbTask = await DbContext.GetDownloadTaskAsync(
                downloadTaskId,
                cancellationToken: CancellationToken.None
            );
            if (dbTask != null && targetStatuses.Contains(dbTask.DownloadStatus))
            {
                _log.Here()
                    .Debug(
                        "Download task {DownloadTaskId} reached status {Status} via database poll",
                        downloadTaskId,
                        dbTask.DownloadStatus
                    );
                return new DownloadProgressDTO
                {
                    Id = dbTask.Id,
                    Title = dbTask.Title,
                    MediaType = dbTask.MediaType,
                    Status = dbTask.DownloadStatus,
                    Percentage = dbTask.Percentage,
                    DataReceived = dbTask.DataReceived,
                    DataTotal = dbTask.DataTotal,
                    DownloadSpeed = dbTask.DownloadSpeed,
                    TimeRemaining = 0,
                    Children = [],
                };
            }

            await Task.Delay(pollInterval, cancellationToken);
        }

        // Final database check with current status logging
        var finalDbTask = await DbContext.GetDownloadTaskAsync(
            downloadTaskId,
            cancellationToken: CancellationToken.None
        );
        if (finalDbTask != null)
        {
            _log.Here()
                .Warning(
                    "Timeout waiting for download task {DownloadTaskId} to reach status {Statuses}. Current status: {CurrentStatus}",
                    downloadTaskId,
                    string.Join(", ", targetStatuses),
                    finalDbTask.DownloadStatus
                );
        }

        return null;
    }

    /// <summary>
    /// Waits for all specified download tasks to reach one of the target statuses.
    /// </summary>
    public bool WaitForAllDownloadsStatusAsync(
        Guid[] downloadTaskIds,
        DownloadStatus[] targetStatuses,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default
    )
    {
        timeout ??= TimeSpan.FromSeconds(30);
        var completedTasks = new HashSet<Guid>();

        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(timeout.Value);

        try
        {
            while (!cts.Token.IsCancellationRequested && completedTasks.Count < downloadTaskIds.Length)
            {
                if (MockSignalRService.ServerDownloadProgressList.TryTake(out var progress, 100, cts.Token))
                {
                    foreach (var taskId in downloadTaskIds.Where(id => !completedTasks.Contains(id)))
                    {
                        var download = FindDownloadById(progress.Downloads, taskId);
                        if (download != null && targetStatuses.Contains(download.Status))
                        {
                            completedTasks.Add(taskId);
                            _log.Here()
                                .Debug(
                                    "Download task {DownloadTaskId} reached status {Status} ({Completed}/{Total})",
                                    taskId,
                                    download.Status,
                                    completedTasks.Count,
                                    downloadTaskIds.Length
                                );
                        }
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _log.Here()
                .Warning(
                    "Timeout waiting for downloads. Completed: {Completed}/{Total}",
                    completedTasks.Count,
                    downloadTaskIds.Length
                );
        }

        return completedTasks.Count == downloadTaskIds.Length;
    }

    private static DownloadProgressDTO? FindDownloadById(List<DownloadProgressDTO> downloads, Guid id)
    {
        foreach (var download in downloads)
        {
            if (download.Id == id)
                return download;

            var child = FindDownloadById(download.Children, id);
            if (child != null)
                return child;
        }

        return null;
    }

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

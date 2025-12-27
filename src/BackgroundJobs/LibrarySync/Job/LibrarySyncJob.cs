using Microsoft.EntityFrameworkCore;
using Quartz;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.Domain;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Quartz job that syncs a single library and chains to the next library if provided.
/// Uses per-server locking to ensure only one library sync runs per server at a time.
/// </summary>
[DisallowConcurrentExecution]
public class LibrarySyncJob : IJob
{
    public const string ServerIdParameter = nameof(ServerIdParameter);
    public const string LibraryIdParameter = nameof(LibraryIdParameter);

    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ISignalRService _signalRService;
    private readonly IReaparrDbContext _dbContext;
    private int _serverId;
    private int _libraryId;

    public LibrarySyncJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        ISignalRService signalRService,
        IReaparrDbContext dbContext
    )
    {
        _log = log.ForContext<LibrarySyncJob>();
        _commandExecutor = commandExecutor;
        _signalRService = signalRService;
        _dbContext = dbContext;
    }

    public static JobKey GetJobKey(int serverId, int libraryId) =>
        new($"{nameof(JobTypes.LibrarySyncJob)}_{serverId}_{libraryId}", nameof(JobTypes.LibrarySyncJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var cancellationToken = context.CancellationToken;

        if (!dataMap.ContainsKey(ServerIdParameter) || !dataMap.ContainsKey(LibraryIdParameter))
        {
            _log.Here()
                .Error(
                    "Missing required parameters in job data map. ServerId: {ServerId}, LibraryId: {LibraryId}",
                    dataMap.ContainsKey(ServerIdParameter),
                    dataMap.ContainsKey(LibraryIdParameter)
                );
            return;
        }

        _serverId = dataMap.GetInt(ServerIdParameter);
        _libraryId = dataMap.GetInt(LibraryIdParameter);

        _log.Here()
            .Debug(
                "Executing job: {LibrarySyncJobName} for server {ServerId}, library {LibraryId}",
                nameof(LibrarySyncJob),
                _serverId,
                _libraryId
            );

        // Check if the server is online before starting sync
        var isServerOnline = await _dbContext.IsServerOnline(_serverId, cancellationToken);
        if (!isServerOnline)
        {
            var serverName = await _dbContext.GetPlexServerNameById(_serverId, cancellationToken);
            _log.Here()
                .Warning(
                    "Server {ServerName} with id {ServerId} is offline, marking queue item and skipping sync",
                    serverName,
                    _serverId
                );
            await UpdateQueueItemAsync(LibrarySyncJobStatus.Queued, isServerOffline: true);
            return;
        }

        await UpdateQueueItemAsync(LibrarySyncJobStatus.Processing);

        // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
        // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
        try
        {
            // Create a progress action that sends individual library progress updates
            var progress = new Action<LibraryProgress>(libraryProgress =>
            {
                _signalRService.SendLibraryProgressUpdateAsync(libraryProgress);
            });

            // Execute the library sync command
            var result = await _commandExecutor.Send(
                new RefreshLibraryMediaCommand(_libraryId, progress),
                context.CancellationToken
            );

            if (result.IsFailed)
            {
                result.LogError();

                // Check if failure was due to the server being offline (504 Gateway Timeout)
                var isServerOffline = result.ToResult().Has504GatewayTimeoutError();

                await UpdateQueueItemAsync(
                    LibrarySyncJobStatus.Failed,
                    errorMessage: result.Errors.FirstOrDefault()?.Message,
                    isServerOffline: isServerOffline
                );

                _log.Here()
                    .Warning(
                        "Library sync failed for server {ServerId}, library {LibraryId}. Queue item marked as failed. Server offline: {IsServerOffline}",
                        _serverId,
                        _libraryId,
                        isServerOffline
                    );
                return;
            }

            _log.Here()
                .Information("Successfully synced library {LibraryId} for server {ServerId}", _libraryId, _serverId);

            // Mark queue item as completed
            await UpdateQueueItemAsync(LibrarySyncJobStatus.Completed);

            // Send refresh notification
            await _signalRService.SendRefreshNotificationAsync([RefreshDataType.PlexLibrary], cancellationToken);

            // Schedule the next library from the queue
            await _commandExecutor.Send(new CheckQueuedPlexLibraryToSyncCommand(), cancellationToken);

            // Queue metadata sync for this server to fetch detailed Part/Stream data
            await _commandExecutor.Send(new QueueMetadataSyncCommand(_serverId), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await UpdateQueueItemAsync(LibrarySyncJobStatus.Queued);

            _log.Here()
                .Information(
                    "{LibrarySyncJobName} for server {ServerId}, library {LibraryId} has been cancelled",
                    nameof(LibrarySyncJob),
                    _serverId,
                    _libraryId
                );
        }
        catch (Exception e)
        {
            await UpdateQueueItemAsync(LibrarySyncJobStatus.Failed, errorMessage: e.Message);

            _log.Here().ErrorResult(e);
        }
    }

    private async Task UpdateQueueItemAsync(
        LibrarySyncJobStatus status,
        string? errorMessage = null,
        bool isServerOffline = false
    )
    {
        var query = _dbContext.LibrarySyncJobQueues.Where(x =>
            x.PlexServerId == _serverId && x.PlexLibraryId == _libraryId
        );

        switch (status)
        {
            case LibrarySyncJobStatus.Queued:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.StartedAt, (DateTime?)null)
                        .SetProperty(x => x.ErrorMessage, (string?)null)
                        .SetProperty(x => x.IsServerOffline, isServerOffline)
                );
                break;

            case LibrarySyncJobStatus.Processing:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.StartedAt, DateTime.UtcNow)
                        .SetProperty(x => x.IsServerOffline, false)
                );
                break;

            case LibrarySyncJobStatus.Completed:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                        .SetProperty(x => x.IsServerOffline, false)
                );
                break;

            case LibrarySyncJobStatus.Failed:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                        .SetProperty(x => x.ErrorMessage, errorMessage)
                        .SetProperty(x => x.IsServerOffline, isServerOffline)
                );
                break;

            case LibrarySyncJobStatus.Unknown:
                _log.Here()
                    .Warning(
                        "Attempted to set LibrarySyncJobStatus to Unknown for server {ServerId}, library {LibraryId}",
                        _serverId,
                        _libraryId
                    );
                break;
            default:
                _log.Here()
                    .Warning(
                        "Attempted to set LibrarySyncJobStatus to invalid value {Status} for server {ServerId}, library {LibraryId}",
                        status,
                        _serverId,
                        _libraryId
                    );
                break;
        }

        await _signalRService.SendRefreshNotificationAsync([RefreshDataType.PlexLibrarySyncStatus]);
    }
}

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

        await UpdateQueueItemAsync(LibrarySyncQueueStatus.Processing);

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
                await UpdateQueueItemAsync(
                    LibrarySyncQueueStatus.Failed,
                    errorMessage: result.Errors.FirstOrDefault()?.Message
                );

                _log.Here()
                    .Warning(
                        "Library sync failed for server {ServerId}, library {LibraryId}. Queue item marked as failed.",
                        _serverId,
                        _libraryId
                    );
                return;
            }

            _log.Here()
                .Information("Successfully synced library {LibraryId} for server {ServerId}", _libraryId, _serverId);

            // Mark queue item as completed
            await UpdateQueueItemAsync(LibrarySyncQueueStatus.Completed);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Send refresh notification
            await _signalRService.SendRefreshNotificationAsync(RefreshDataType.PlexLibrary, cancellationToken);

            // Schedule the next library from the queue
            await _commandExecutor.Send(new CheckQueuedPlexLibraryToSyncCommand(), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            await UpdateQueueItemAsync(LibrarySyncQueueStatus.Queued);

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
            await UpdateQueueItemAsync(LibrarySyncQueueStatus.Failed, errorMessage: e.Message);

            _log.Here().ErrorResult(e);
        }
    }

    private async Task UpdateQueueItemAsync(LibrarySyncQueueStatus status, string? errorMessage = null)
    {
        var query = _dbContext.LibrarySyncJobQueues.Where(x =>
            x.PlexServerId == _serverId && x.PlexLibraryId == _libraryId
        );

        switch (status)
        {
            case LibrarySyncQueueStatus.Processing:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status).SetProperty(x => x.StartedAt, DateTime.UtcNow)
                );
                break;

            case LibrarySyncQueueStatus.Completed:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status).SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                );
                break;

            case LibrarySyncQueueStatus.Failed:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                        .SetProperty(x => x.ErrorMessage, errorMessage)
                );
                break;

            case LibrarySyncQueueStatus.Queued:
                await query.ExecuteUpdateAsync(s =>
                    s.SetProperty(x => x.Status, status)
                        .SetProperty(x => x.StartedAt, (DateTime?)null)
                        .SetProperty(x => x.ErrorMessage, (string?)null)
                );
                break;
            case LibrarySyncQueueStatus.Unknown:
                throw new ArgumentOutOfRangeException(nameof(status), "Cannot set status to Unknown");
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }
}

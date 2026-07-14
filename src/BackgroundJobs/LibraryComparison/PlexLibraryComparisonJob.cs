namespace Reaparr.BackgroundJobs;

/// <summary>
/// Singleton Quartz worker that drains persisted library comparison queue rows.
/// </summary>
/// <remarks>
/// Each execution claims one queued remote-to-owned library pair, runs the media-specific comparison command, updates the
/// queue row, and re-schedules itself when more persisted work remains.
/// </remarks>
[DisallowConcurrentExecution]
public class PlexLibraryComparisonJob : IJob
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public PlexLibraryComparisonJob(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<PlexLibraryComparisonJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public static JobKey GetJobKey() => new(nameof(JobTypes.LibraryComparisonJob), nameof(JobTypes.LibraryComparisonJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        // Claim one persisted row per run to bound database and Plex comparison load.
        var queueItem = await _dbContext.LibraryComparisonJobQueues
            .Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (queueItem is null)
        {
            _log.Here().Debug("No queued library comparison jobs found");
            return;
        }

        await _dbContext.LibraryComparisonJobQueues
            .Where(x =>
                x.RemotePlexLibraryId == queueItem.RemotePlexLibraryId
                && x.OwnedPlexLibraryId == queueItem.OwnedPlexLibraryId
                && x.MediaType == queueItem.MediaType
            )
            .ExecuteUpdateAsync(
                x => x.SetProperty(y => y.Status, LibrarySyncJobStatus.Processing)
                    .SetProperty(y => y.StartedAt, DateTime.UtcNow)
                    .SetProperty(y => y.CompletedAt, (DateTime?)null)
                    .SetProperty(y => y.Attempts, y => y.Attempts + 1)
                    .SetProperty(y => y.ErrorMessage, (string?)null),
                cancellationToken
            );

        _log.Here()
            .Debug(
                "Executing comparison queue item: remote library {RemoteLibId} vs owned library {OwnedLibId} for {MediaType}",
                queueItem.RemotePlexLibraryId,
                queueItem.OwnedPlexLibraryId,
                queueItem.MediaType
            );

        var result = queueItem.MediaType switch
        {
            PlexMediaType.Movie => await _commandExecutor.Send(
                new CompareMoviePlexLibraryCommand(queueItem.RemotePlexLibraryId, queueItem.OwnedPlexLibraryId),
                cancellationToken
            ),
            PlexMediaType.TvShow => await _commandExecutor.Send(
                new CompareTvShowPlexLibraryCommand(queueItem.RemotePlexLibraryId, queueItem.OwnedPlexLibraryId),
                cancellationToken
            ),
            _ => Result.Fail($"Library comparison for media type {queueItem.MediaType} is not yet implemented"),
        };

        if (result.IsFailed)
        {
            result.LogError();
            await UpdateQueueItemAsync(queueItem, LibrarySyncJobStatus.Failed, result.ToString(), cancellationToken);
            _log.Here()
                .Warning(
                    "Comparison queue item failed for remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                    queueItem.RemotePlexLibraryId,
                    queueItem.OwnedPlexLibraryId,
                    queueItem.MediaType
                );
        }
        else
        {
            await UpdateQueueItemAsync(queueItem, LibrarySyncJobStatus.Completed, null, cancellationToken);
            _log.Here()
                .Information(
                    "Comparison queue item completed for remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                    queueItem.RemotePlexLibraryId,
                    queueItem.OwnedPlexLibraryId,
                    queueItem.MediaType
                );
        }

        var hasMoreQueuedItems = await _dbContext.LibraryComparisonJobQueues
            .AnyAsync(x => x.Status == LibrarySyncJobStatus.Queued, cancellationToken);

        if (hasMoreQueuedItems)
            await _commandExecutor.Send(new CheckQueuedLibraryComparisonJobCommand(), cancellationToken);
    }

    private async Task UpdateQueueItemAsync(
        LibraryComparisonJobQueue queueItem,
        LibrarySyncJobStatus status,
        string? errorMessage,
        CancellationToken cancellationToken
    )
    {
        await _dbContext.LibraryComparisonJobQueues
            .Where(x =>
                x.RemotePlexLibraryId == queueItem.RemotePlexLibraryId
                && x.OwnedPlexLibraryId == queueItem.OwnedPlexLibraryId
                && x.MediaType == queueItem.MediaType
            )
            .ExecuteUpdateAsync(
                x => x.SetProperty(y => y.Status, status)
                    .SetProperty(y => y.CompletedAt, DateTime.UtcNow)
                    .SetProperty(y => y.ErrorMessage, errorMessage),
                cancellationToken
            );
    }
}

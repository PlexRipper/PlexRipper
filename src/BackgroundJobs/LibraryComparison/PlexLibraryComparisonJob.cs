namespace Reaparr.BackgroundJobs;

/// <summary>
/// Singleton Quartz worker that drains persisted library comparison queue rows.
/// </summary>
/// <remarks>
/// Each execution drains queued remote-to-owned library pairs one at a time until no persisted work remains.
/// </remarks>
[DisallowConcurrentExecution]
public class PlexLibraryComparisonJob : IJob
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IProgressHubService _progressHubService;

    public PlexLibraryComparisonJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService
    )
    {
        _log = log.ForContext<PlexLibraryComparisonJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressHubService = progressHubService;
    }

    public static JobKey GetJobKey() => new(nameof(JobTypes.LibraryComparisonJob), nameof(JobTypes.LibraryComparisonJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var processedCount = 0;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _log.Here().Warning("Library comparison queue worker was cancelled");
                return;
            }

            var processedQueueItem = await ProcessNextQueueItemAsync(cancellationToken);

            if (!processedQueueItem)
                break;

            processedCount++;
        }

        _log.Here().Debug("Library comparison queue worker finished after processing {Count} items", processedCount);
    }

    private async Task<bool> ProcessNextQueueItemAsync(CancellationToken cancellationToken)
    {
        var queueItem = await _dbContext.LibraryComparisonJobQueues
            .Where(x => x.Status == LibrarySyncJobStatus.Queued)
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (queueItem is null)
        {
            _log.Here().Debug("No queued library comparison jobs found");
            return false;
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

        if (result.IsCancelled)
        {
            await UpdateQueueItemAsync(
                queueItem,
                LibrarySyncJobStatus.Queued,
                "Library comparison was cancelled and requeued",
                CancellationToken.None
            );
            _log.Here()
                .Warning(
                    "Comparison queue item was cancelled and requeued for remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                    queueItem.RemotePlexLibraryId,
                    queueItem.OwnedPlexLibraryId,
                    queueItem.MediaType
                );
            return false;
        }

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

        await SendCompletionNotificationIfSettledAsync(queueItem, cancellationToken);
        return true;
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

    private async Task SendCompletionNotificationIfSettledAsync(
        LibraryComparisonJobQueue queueItem,
        CancellationToken cancellationToken
    )
    {
        var affectedLibraryIds = new List<int>();

        if (await IsRemoteLibrarySettledAsync(queueItem, cancellationToken))
            affectedLibraryIds.Add(queueItem.RemotePlexLibraryId);

        if (await IsOwnedLibrarySettledAsync(queueItem, cancellationToken))
            affectedLibraryIds.Add(queueItem.OwnedPlexLibraryId);

        if (affectedLibraryIds.Count == 0)
            return;

        await _progressHubService.SendLibraryComparisonCompletedAsync(
            new LibraryComparisonCompletedDTO
            {
                AffectedLibraryIds = affectedLibraryIds.Distinct().ToList(),
                MediaType = queueItem.MediaType,
                CompletedAt = DateTime.UtcNow,
            },
            cancellationToken
        );
    }

    private async Task<bool> IsRemoteLibrarySettledAsync(
        LibraryComparisonJobQueue queueItem,
        CancellationToken cancellationToken
    ) =>
        !await _dbContext.LibraryComparisonJobQueues.AnyAsync(
            x => x.RemotePlexLibraryId == queueItem.RemotePlexLibraryId
                 && x.MediaType == queueItem.MediaType
                 && (x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing),
            cancellationToken
        );

    private async Task<bool> IsOwnedLibrarySettledAsync(
        LibraryComparisonJobQueue queueItem,
        CancellationToken cancellationToken
    ) =>
        !await _dbContext.LibraryComparisonJobQueues.AnyAsync(
            x => x.OwnedPlexLibraryId == queueItem.OwnedPlexLibraryId
                 && x.MediaType == queueItem.MediaType
                 && (x.Status == LibrarySyncJobStatus.Queued || x.Status == LibrarySyncJobStatus.Processing),
            cancellationToken
        );
}

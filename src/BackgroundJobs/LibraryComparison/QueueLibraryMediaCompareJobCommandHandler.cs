namespace Reaparr.BackgroundJobs;

public class QueueLibraryMediaCompareJobCommandValidator : AbstractValidator<QueueLibraryMediaCompareJobCommand>
{
    public QueueLibraryMediaCompareJobCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.RemotePlexLibraryId).GreaterThan(0);
        RuleFor(x => x.OwnedPlexLibraryId).GreaterThan(0);
        RuleFor(x => x.MediaType).IsInEnum();
    }
}

/// <summary>
/// Persists one de-duplicated library comparison queue row and wakes the queue worker.
/// </summary>
public class QueueLibraryMediaCompareJobCommandHandler : ICommandHandler<QueueLibraryMediaCompareJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public QueueLibraryMediaCompareJobCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<QueueLibraryMediaCompareJobCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result> ExecuteAsync(
        QueueLibraryMediaCompareJobCommand command,
        CancellationToken cancellationToken
    )
    {
        var (remoteLibraryId, ownedLibraryId, mediaType) = command;

        var existingQueueItem = await _dbContext.LibraryComparisonJobQueues
            .FirstOrDefaultAsync(
                x =>
                    x.RemotePlexLibraryId == remoteLibraryId
                    && x.OwnedPlexLibraryId == ownedLibraryId
                    && x.MediaType == mediaType,
                cancellationToken
            );

        if (existingQueueItem is null)
        {
            await _dbContext.LibraryComparisonJobQueues.AddAsync(
                new LibraryComparisonJobQueue
                {
                    RemotePlexLibraryId = remoteLibraryId,
                    OwnedPlexLibraryId = ownedLibraryId,
                    MediaType = mediaType,
                    Priority = mediaType == PlexMediaType.Movie ? 1 : 2,
                    Status = LibrarySyncJobStatus.Queued,
                    CreatedAt = DateTime.UtcNow,
                },
                cancellationToken
            );
            await _dbContext.SaveChangesNewAsync(cancellationToken);
        }
        // Leave queued or processing rows alone; they already represent the latest requested work.
        else if (existingQueueItem.Status is LibrarySyncJobStatus.Completed or LibrarySyncJobStatus.Failed or LibrarySyncJobStatus.Cancelled)
        {
            await _dbContext.LibraryComparisonJobQueues
                .Where(x =>
                    x.RemotePlexLibraryId == remoteLibraryId
                    && x.OwnedPlexLibraryId == ownedLibraryId
                    && x.MediaType == mediaType
                )
                .ExecuteUpdateAsync(
                    x => x.SetProperty(y => y.Status, LibrarySyncJobStatus.Queued)
                        .SetProperty(y => y.CreatedAt, DateTime.UtcNow)
                        .SetProperty(y => y.StartedAt, (DateTime?)null)
                        .SetProperty(y => y.CompletedAt, (DateTime?)null)
                        .SetProperty(y => y.ErrorMessage, (string?)null),
                    cancellationToken
                );
        }

        await _commandExecutor.Send(new CheckQueuedLibraryComparisonJobCommand(), cancellationToken);

        _log.Here()
            .Debug(
                "Scheduled library comparison job: remote {RemoteLibId} vs owned {OwnedLibId}, {MediaType}",
                remoteLibraryId,
                ownedLibraryId,
                mediaType
            );

        return Result.Ok();
    }
}

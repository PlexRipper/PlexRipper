namespace Reaparr.Application;

/// <summary>
/// Normalizes persisted library comparison queue rows during scheduler startup.
/// </summary>
/// <remarks>
/// Completed rows are disposable, while interrupted or failed rows are made queued again so shutdowns do not lose work.
/// </remarks>
public record CleanupLibraryComparisonJobQueueCommand : ICommand<Result>;


/// <summary>
/// Validates startup cleanup requests for the persisted library comparison queue.
/// </summary>
public class CleanupLibraryComparisonJobQueueCommandValidator
    : AbstractValidator<CleanupLibraryComparisonJobQueueCommand>
{
    public CleanupLibraryComparisonJobQueueCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

/// <summary>
/// Cleans persisted comparison queue rows so startup can safely resume interrupted work.
/// </summary>
public class CleanupLibraryComparisonJobQueueCommandHandler
    : ICommandHandler<CleanupLibraryComparisonJobQueueCommand, Result>
{
    private const int MAX_ATTEMPTS = 3;
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public CleanupLibraryComparisonJobQueueCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<CleanupLibraryComparisonJobQueueCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(
        CleanupLibraryComparisonJobQueueCommand command,
        CancellationToken cancellationToken
    )
    {
        await _dbContext.LibraryComparisonJobQueues
            .Where(x => x.Status == LibrarySyncJobStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext.LibraryComparisonJobQueues
            .Where(x => x.Status == LibrarySyncJobStatus.Processing && x.Attempts >= MAX_ATTEMPTS)
            .ExecuteUpdateAsync(
                x => x.SetProperty(y => y.Status, LibrarySyncJobStatus.Failed)
                    .SetProperty(y => y.CompletedAt, DateTime.UtcNow)
                    .SetProperty(y => y.ErrorMessage, "Library comparison exceeded retry attempts while processing"),
                cancellationToken
            );

        await _dbContext.LibraryComparisonJobQueues
            .Where(x => (x.Status == LibrarySyncJobStatus.Processing || x.Status == LibrarySyncJobStatus.Failed) && x.Attempts < MAX_ATTEMPTS)
            .ExecuteUpdateAsync(
                x => x.SetProperty(y => y.Status, LibrarySyncJobStatus.Queued)
                    .SetProperty(y => y.StartedAt, (DateTime?)null)
                    .SetProperty(y => y.CompletedAt, (DateTime?)null)
                    .SetProperty(y => y.ErrorMessage, (string?)null),
                cancellationToken
            );

        _log.Here().Debug("Cleaned up library comparison job queue");

        return Result.Ok();
    }
}
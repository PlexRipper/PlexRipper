namespace Reaparr.Application;
/// <summary>
/// This is a command to clean up the library sync job queue by removing completed or failed jobs.
/// NOTE: This should only be run once on startup to ensure the queue is clean.
/// </summary>
public record CleanupLibrarySyncJobQueueCommand : ICommand<Result>;

public class CleanupLibrarySyncJobQueueCommandValidator : AbstractValidator<CleanupLibrarySyncJobQueueCommand>
{
    public CleanupLibrarySyncJobQueueCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CleanupLibrarySyncJobQueueCommandHandler : ICommandHandler<CleanupLibrarySyncJobQueueCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly INotificationHubService _notificationHubService;

    public CleanupLibrarySyncJobQueueCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<CleanupLibrarySyncJobQueueCommandHandler>();
        _dbContext = dbContext;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result> ExecuteAsync(
        CleanupLibrarySyncJobQueueCommand command,
        CancellationToken cancellationToken
    )
    {
        // Cleanup completed queue items
        await _dbContext
            .LibrarySyncJobQueues.Where(x => x.Status == LibrarySyncJobStatus.Completed)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        // Re-queue failed or processing items
        await _dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.Status == LibrarySyncJobStatus.Failed || x.Status == LibrarySyncJobStatus.Processing
            )
            .ResetJobsToQueuedAsync(cancellationToken);

        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexLibrarySyncStatus]
        );

        _log.Here().Debug("Cleaned up library sync job queue.");

        return Result.Ok();
    }
}

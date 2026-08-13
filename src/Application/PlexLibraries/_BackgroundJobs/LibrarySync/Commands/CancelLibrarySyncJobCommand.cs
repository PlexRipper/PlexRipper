namespace Reaparr.Application;

public record CancelLibrarySyncJobCommand(int PlexLibraryId) : ICommand<Result>;

public class CancelLibrarySyncJobCommandValidator : AbstractValidator<CancelLibrarySyncJobCommand>
{
    public CancelLibrarySyncJobCommandValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class CancelLibrarySyncJobCommandHandler : ICommandHandler<CancelLibrarySyncJobCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IBackgroundJobScheduler _scheduler;
    private readonly INotificationHubService _notificationHubService;

    public CancelLibrarySyncJobCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IBackgroundJobScheduler scheduler,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<CancelLibrarySyncJobCommandHandler>();
        _dbContext = dbContext;
        _scheduler = scheduler;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result> ExecuteAsync(CancelLibrarySyncJobCommand command, CancellationToken cancellationToken)
    {
        var plexLibraryId = command.PlexLibraryId;

        var queueItem = await _dbContext
            .LibrarySyncJobQueues.Where(x => x.PlexLibraryId == plexLibraryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (queueItem is null)
        {
            _log.Here()
                .Warning(
                    "No library sync queue item found for library {PlexLibraryId}, nothing to cancel",
                    plexLibraryId
                );
            return Result.Ok();
        }

        if (queueItem.Status is LibrarySyncJobStatus.Completed or LibrarySyncJobStatus.Cancelled)
        {
            _log.Here()
                .Warning(
                    "Library sync job for library {PlexLibraryId} has status {Status}, nothing to cancel",
                    plexLibraryId,
                    queueItem.Status
                );
            return Result.Ok();
        }

        var serverId = queueItem.PlexServerId;
        var jobKey = LibrarySyncJob.GetJobKey(serverId, plexLibraryId);

        var wasInterrupted = await _scheduler.Interrupt(jobKey, cancellationToken);
        if (wasInterrupted)
        {
            _log.Here()
                .Information(
                    "Interrupted running library sync job for server {ServerId}, library {PlexLibraryId}",
                    serverId,
                    plexLibraryId
                );
        }
        else
        {
            // Job is queued but not yet executing — mark it as cancelled directly.
            await _dbContext
                .LibrarySyncJobQueues.Where(x => x.PlexServerId == serverId && x.PlexLibraryId == plexLibraryId)
                .ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(x => x.Status, LibrarySyncJobStatus.Cancelled)
                            .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                            .SetProperty(x => x.ErrorMessage, (string?)null)
                            .SetProperty(x => x.IsServerOffline, false),
                    cancellationToken: cancellationToken
                );

            _log.Here()
                .Information(
                    "Cancelled queued library sync job for server {ServerId}, library {PlexLibraryId}",
                    serverId,
                    plexLibraryId
                );

            await _notificationHubService.SendRefreshNotificationAsync(
                [RefreshDataType.PlexLibrarySyncStatus]
            );
        }

        return Result.Ok();
    }
}

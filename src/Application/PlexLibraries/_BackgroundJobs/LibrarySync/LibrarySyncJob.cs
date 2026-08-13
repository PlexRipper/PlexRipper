using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public record LibrarySyncJobPayload
{
    public required int PlexServerId { get; init; }

    public required int PlexLibraryId { get; init; }
}

/// <summary>
/// TickerQ job that syncs a single library and chains to the next queued library.
/// Uses per-server locking to ensure only one library sync runs per server at a time.
/// </summary>
public class LibrarySyncJob : BaseBackgroundJob<LibrarySyncJobPayload, LibrarySyncJobQueueDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly ICommandExecutor _commandExecutor;
    private readonly INotificationHubService _notificationHubService;
    private readonly IBackgroundJobScheduler _backgroundJobScheduler;
    private readonly IReaparrDbContext _dbContext;
    private bool _skipAfterCompletion;

    protected override JobTypes JobType => JobTypes.LibrarySyncJob;

    protected override List<RefreshDataType> RefreshDataTypes =>
        [RefreshDataType.PlexLibrary, RefreshDataType.PlexLibrarySyncStatus];

    public LibrarySyncJob(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService,
        IProgressHubService progressHubService,
        IBackgroundJobScheduler backgroundJobScheduler
    ) : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<LibrarySyncJob>();
        _dbContextFactory = dbContextFactory;
        _dbContext = dbContextFactory.Create();
        _commandExecutor = commandExecutor;
        _notificationHubService = notificationHubService;
        _backgroundJobScheduler = backgroundJobScheduler;
    }

    public static JobKey GetJobKey(int serverId, int libraryId) =>
        new($"{nameof(JobTypes.LibrarySyncJob)}_{serverId}_{libraryId}", JobTypes.LibrarySyncJob);

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<LibrarySyncJobPayload> context,
        CancellationToken cancellationToken)
    {
        _skipAfterCompletion = false;

        var serverId = context.Request.PlexServerId;
        var libraryId = context.Request.PlexLibraryId;

        var serverName = await _dbContext.GetPlexServerNameById(serverId);
        var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId);

        _log.Here()
            .Debug(
                "Executing job: {LibrarySyncJobName} for server {ServerName} with id {ServerId} and library {LibraryName} with id {LibraryId}",
                nameof(LibrarySyncJob),
                serverName,
                serverId,
                libraryName,
                libraryId
            );

        // Check if the server is online before starting sync
        var isServerOnline = await _dbContext.IsServerOnline(serverId);
        if (!isServerOnline)
        {
            _log.Here()
                .Warning(
                    "Server {ServerName} with id {ServerId} is offline, marking queue item and skipping sync",
                    serverName,
                    serverId
                );
            await UpdateQueueItemAsync(context,
                LibrarySyncJobStatus.Queued,
                isServerOffline: true
            );
        }
        else
        {
            if (!await TryClaimQueueItemForProcessingAsync(serverId, libraryId))
            {
                _skipAfterCompletion = true;
                return;
            }

            var invalidationResult = await _commandExecutor.Send(
                new InvalidateLibraryComparisonJobsCommand([libraryId]),
                cancellationToken
            );

            invalidationResult.LogIfFailed();

            // Convert command exceptions to a Result so the queue state can be persisted before this ticker completes.
            // Execute the library sync command
            var result = await Result.Try(() =>
                _commandExecutor.Send(new RefreshLibraryMediaCommand(libraryId), cancellationToken)
            );

            if (result.IsCancelled)
            {
                _log.Here()
                    .Information(
                        "{LibrarySyncJobName} for server {ServerId}, library {LibraryId} has been cancelled",
                        nameof(LibrarySyncJob),
                        serverId,
                        libraryId
                    );

                await UpdateQueueItemAsync(context, LibrarySyncJobStatus.Cancelled);
                await _notificationHubService.SendRefreshNotificationAsync([RefreshDataType.PlexLibrary]);

                return;
            }

            if (result.IsFailed)
            {
                result.LogError();

                var syncResult = result.ToResult();

                if (syncResult.HasPlex401UnauthorizedError() || syncResult.Has401UnauthorizedError())
                {
                    _log.Here()
                        .Warning(
                            "Library sync failed for server {ServerId}, library {LibraryId} because the Plex token is unauthorized. Dequeuing library sync job.",
                            serverId,
                            libraryId
                        );

                    var deleteResult = await _backgroundJobScheduler.DeleteBatchJobs(
                        [GetJobKey(serverId, libraryId)],
                        cancellationToken
                    );

                    if (deleteResult.IsFailed)
                        deleteResult.LogWarning();
                }

                // Check if failure was due to the server being offline (504 Gateway Timeout)
                // TODO make "Server offline" a generic FluentResult check as this can happen in other places as well and we want to handle it consistently across the app
                var isServerOffline = syncResult.Has504GatewayTimeoutError();

                _log.Here()
                    .Warning(
                        "Library sync failed for server {ServerId}, library {LibraryId}. Queue item marked as failed. Server offline: {IsServerOffline}",
                        serverId,
                        libraryId,
                        isServerOffline
                    );

                await UpdateQueueItemAsync(
                    context,
                    LibrarySyncJobStatus.Failed,
                    errorMessage: result.Errors.FirstOrDefault()?.Message,
                    isServerOffline: isServerOffline
                );
            }
            else
            {
                _log.Here()
                    .Information(
                        "Successfully synced library {LibraryId} for server {ServerId}",
                        libraryId,
                        serverId
                    );

                await UpdateQueueItemAsync(context, LibrarySyncJobStatus.Completed);
            }
        }
    }

    protected override async Task ExecuteAfterCompletionAsync(
        TickerFunctionContext<LibrarySyncJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        if (_skipAfterCompletion)
        {
            _log.Here()
                .Debug(
                    "Skipping post-completion work for duplicate or stale library sync job for server {ServerId}, library {LibraryId}",
                    context.Request.PlexServerId,
                    context.Request.PlexLibraryId
                );
            return;
        }

        // Schedule the next library first so expensive comparison scheduling cannot stall the sync queue.
        // Use an independent bounded token because the ticker execution token may already be cancelled.
        using (var queueTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
        {
            var queueResult = await _commandExecutor.Send(
                new CheckQueuedPlexLibraryToSyncCommand(),
                queueTokenSource.Token
            );

            if (queueResult.IsFailed)
                queueResult.LogWarning();
        }

        var libraryId = context.Request.PlexLibraryId;
        var serverId = context.Request.PlexServerId;
        using var dbContext = await _dbContextFactory.CreateAsync();
        var queueStatus = await dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.PlexServerId == serverId && x.PlexLibraryId == libraryId
            )
            .Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);

        if (queueStatus == LibrarySyncJobStatus.Completed)
        {
            var comparisonQueueResult = await _commandExecutor.Send(
                new ScheduleAffectedLibraryComparisonJobsCommand(libraryId),
                cancellationToken
            );

            if (comparisonQueueResult.IsFailed)
                _log.Here()
                    .Warning(
                        "Failed to queue comparison jobs for library {LibraryId}",
                        libraryId
                    );
        }
    }

    private async Task<bool> TryClaimQueueItemForProcessingAsync(int serverId, int libraryId)
    {
        var startedAt = DateTime.UtcNow;
        var updatedRows = await _dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.PlexServerId == serverId
                && x.PlexLibraryId == libraryId
                && (
                    x.Status == LibrarySyncJobStatus.Queued
                    || (x.Status == LibrarySyncJobStatus.Processing && x.StartedAt == null)
                )
            )
            .ExecuteUpdateAsync(
                s =>
                    s.SetProperty(x => x.Status, LibrarySyncJobStatus.Processing)
                        .SetProperty(x => x.StartedAt, startedAt)
                        .SetProperty(x => x.CompletedAt, (DateTime?)null)
                        .SetProperty(x => x.ErrorMessage, (string?)null)
                        .SetProperty(x => x.IsServerOffline, false),
                CancellationToken.None
            );

        if (updatedRows > 0)
            return true;

        var queueState = await _dbContext
            .LibrarySyncJobQueues.Where(x =>
                x.PlexServerId == serverId && x.PlexLibraryId == libraryId
            )
            .Select(x => new { x.Status, x.StartedAt, x.CompletedAt })
            .FirstOrDefaultAsync(CancellationToken.None);

        if (queueState is null)
        {
            _log.Here()
                .Warning(
                    "Skipping library sync job for server {ServerId}, library {LibraryId} because the queue item no longer exists",
                    serverId,
                    libraryId
                );
        }
        else
        {
            _log.Here()
                .Warning(
                    "Skipping duplicate or stale library sync job for server {ServerId}, library {LibraryId}. Queue status: {Status}, started at: {StartedAt}, completed at: {CompletedAt}",
                    serverId,
                    libraryId,
                    queueState.Status,
                    queueState.StartedAt,
                    queueState.CompletedAt
                );
        }

        return false;
    }

    private async Task UpdateQueueItemAsync(
        TickerFunctionContext<LibrarySyncJobPayload> context,
        LibrarySyncJobStatus status,
        string? errorMessage = null,
        bool isServerOffline = false
    )
    {
        var serverId = context.Request.PlexServerId;
        var libraryId = context.Request.PlexLibraryId;

        var query = _dbContext.LibrarySyncJobQueues.Where(x =>
            x.PlexServerId == serverId && x.PlexLibraryId == libraryId
        );

        switch (status)
        {
            case LibrarySyncJobStatus.Queued:
                await query.ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(x => x.Status, status)
                            .SetProperty(x => x.StartedAt, (DateTime?)null)
                            .SetProperty(x => x.ErrorMessage, (string?)null)
                            .SetProperty(x => x.IsServerOffline, isServerOffline),
                    CancellationToken.None
                );
                break;

            case LibrarySyncJobStatus.Processing:
                await query.ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(x => x.Status, status)
                            .SetProperty(x => x.StartedAt, DateTime.UtcNow)
                            .SetProperty(x => x.IsServerOffline, false),
                    CancellationToken.None
                );
                break;

            case LibrarySyncJobStatus.Completed:
                await query.ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(x => x.Status, status)
                            .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                            .SetProperty(x => x.IsServerOffline, false),
                    CancellationToken.None
                );
                break;

            case LibrarySyncJobStatus.Failed:
                await query.ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(x => x.Status, status)
                            .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                            .SetProperty(x => x.ErrorMessage, errorMessage)
                            .SetProperty(x => x.IsServerOffline, isServerOffline),
                    CancellationToken.None
                );
                break;

            case LibrarySyncJobStatus.Cancelled:
                await query.ExecuteUpdateAsync(
                    s =>
                        s.SetProperty(x => x.Status, status)
                            .SetProperty(x => x.CompletedAt, DateTime.UtcNow)
                            .SetProperty(x => x.ErrorMessage, (string?)null)
                            .SetProperty(x => x.IsServerOffline, false),
                    CancellationToken.None
                );
                break;

            case LibrarySyncJobStatus.Unknown:
                _log.Here()
                    .Warning(
                        "Attempted to set LibrarySyncJobStatus to Unknown for server {ServerId}, library {LibraryId}",
                        serverId,
                        libraryId
                    );
                break;
            default:
                _log.Here()
                    .Warning(
                        "Attempted to set LibrarySyncJobStatus to invalid value {Status} for server {ServerId}, library {LibraryId}",
                        status,
                        serverId,
                        libraryId
                    );
                break;
        }
    }

    protected override async Task<LibrarySyncJobQueueDTO?> GetStatusUpdateDataAsync(
        TickerFunctionContext<LibrarySyncJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        // The base job publishes Started before ExecuteJobAsync initializes the
        // instance fields. Always use the payload so the initial SignalR update
        // looks up the actual queue item instead of server/library 0.
        var serverId = context.Request.PlexServerId;
        var libraryId = context.Request.PlexLibraryId;

        using var dbContext = await _dbContextFactory.CreateAsync();
        var queue = await dbContext.LibrarySyncJobQueues.FirstOrDefaultAsync(
            x => x.PlexServerId == serverId && x.PlexLibraryId == libraryId,
            cancellationToken
        );

        if (queue == null)
        {
            _log.Here()
                .Warning(
                    "Queue item not found for server {ServerId}, library {LibraryId} when sending status update",
                    serverId,
                    libraryId
                );
            return null;
        }

        return queue.ToDTO();
    }
}
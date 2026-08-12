using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public record LibrarySyncJobPayload
{
    public int PlexServerId { get; set; }

    public int PlexLibraryId { get; set; }
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
    private readonly IReaparrDbContext _dbContext;

    protected override JobTypes JobType => JobTypes.LibrarySyncJob;

    protected override List<RefreshDataType> RefreshDataTypes =>
        [RefreshDataType.PlexLibrary, RefreshDataType.PlexLibrarySyncStatus];

    public LibrarySyncJob(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService,
        IProgressHubService progressHubService
    ) : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<LibrarySyncJob>();
        _dbContextFactory = dbContextFactory;
        _dbContext = dbContextFactory.Create();
        _commandExecutor = commandExecutor;
        _notificationHubService = notificationHubService;
    }

    public static JobKey GetJobKey(int serverId, int libraryId) =>
        new($"{nameof(JobTypes.LibrarySyncJob)}_{serverId}_{libraryId}", JobTypes.LibrarySyncJob);

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<LibrarySyncJobPayload> context,
        CancellationToken cancellationToken)
    {
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
            await UpdateQueueItemAsync(context, LibrarySyncJobStatus.Processing);

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

                // The TickerQ execution token is already cancelled, so use a short-lived token
                // to persist the cancellation and continue processing the queue.
                using var cleanupTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var cleanupToken = cleanupTokenSource.Token;

                await UpdateQueueItemAsync(context, LibrarySyncJobStatus.Cancelled);
                await _notificationHubService.SendRefreshNotificationAsync([RefreshDataType.PlexLibrary]);
                await _commandExecutor.Send(new CheckQueuedPlexLibraryToSyncCommand(), cleanupToken);

                return;
            }

            if (result.IsFailed)
            {
                result.LogError();

                // Check if failure was due to the server being offline (504 Gateway Timeout)
                // TODO make "Server offline" a generic FluentResult check as this can happen in other places as well and we want to handle it consistently across the app
                var isServerOffline = result.ToResult().Has504GatewayTimeoutError();

                _log.Here()
                    .Warning(
                        "Library sync failed for server {ServerId}, library {LibraryId}. Queue item marked as failed. Server offline: {IsServerOffline}",
                        serverId,
                        libraryId,
                        isServerOffline
                    );

                await UpdateQueueItemAsync(context,
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

        // Send PlexLibrary refresh notification
        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexLibrary]
        );

        // Schedule the next library from the queue
        await _commandExecutor.Send(new CheckQueuedPlexLibraryToSyncCommand(), cancellationToken);
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
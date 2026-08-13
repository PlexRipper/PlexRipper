using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public record CheckPlexLibrariesForUpdatesJobPayload;

public record CheckPlexLibrariesForUpdatesJobUpdate;

/// <summary>
/// Periodically checks whether Plex libraries changed and queues full library syncs only when needed.
/// </summary>
public class CheckPlexLibrariesForUpdatesJob
    : BaseBackgroundJob<CheckPlexLibrariesForUpdatesJobPayload, CheckPlexLibrariesForUpdatesJobUpdate>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public CheckPlexLibrariesForUpdatesJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        INotificationHubService notificationHubService,
        IProgressHubService progressHubService
    )
        : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<CheckPlexLibrariesForUpdatesJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    protected override JobTypes JobType => JobTypes.CheckPlexLibrariesForUpdatesJob;

    public static JobKey GetJobKey() => new(
        nameof(JobTypes.CheckPlexLibrariesForUpdatesJob),
        JobTypes.CheckPlexLibrariesForUpdatesJob
    );

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<CheckPlexLibrariesForUpdatesJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        _log.Here().Debug("Executing job: {JobName}", nameof(CheckPlexLibrariesForUpdatesJob));

        var enabledServerIds = await _dbContext.PlexServers
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (enabledServerIds.Count == 0)
        {
            _log.Here().Debug("No enabled Plex servers found for automatic library sync");
            return;
        }

        var accountMappings = await _dbContext.PlexAccountServers
            .Where(x => enabledServerIds.Contains(x.PlexServerId))
            .GroupBy(x => x.PlexServerId)
            .Select(x => new
            {
                PlexServerId = x.Key,
                PlexAccountId = x.Min(y => y.PlexAccountId),
            })
            .ToDictionaryAsync(x => x.PlexServerId, x => x.PlexAccountId, cancellationToken);

        var serversWithTokenMappings = new List<int>();
        foreach (var serverId in enabledServerIds)
        {
            if (!accountMappings.TryGetValue(serverId, out var accountId) || accountId <= 0)
            {
                _log.Here()
                    .Warning(
                        "No Plex account-server token mapping found for PlexServer with id {PlexServerId}; skipping refresh",
                        serverId
                    );
                continue;
            }

            var refreshResult = await _commandExecutor.Send(
                new RefreshLibraryAccessCommand(accountId, serverId),
                cancellationToken
            );

            if (refreshResult.IsCancelled)
                throw new OperationCanceledException(cancellationToken);

            if (refreshResult.IsFailed)
            {
                refreshResult.ToResult().LogError();
                continue;
            }

            serversWithTokenMappings.Add(serverId);
        }

        if (serversWithTokenMappings.Count == 0)
        {
            _log.Here().Debug("No Plex servers with token mappings found for automatic library sync");
            return;
        }

        var outdatedLibraryIds = await _dbContext.PlexLibraries
            .AsNoTracking()
            .Where(x => serversWithTokenMappings.Contains(x.PlexServerId))
            .Where(x => x.Type == PlexMediaType.Movie || x.Type == PlexMediaType.TvShow)
            .Where(x => x.Outdated)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        if (outdatedLibraryIds.Count == 0)
        {
            _log.Here().Debug("No outdated Plex libraries found for automatic sync");
            return;
        }

        _log.Here()
            .Information("Queueing {Count} outdated Plex libraries for automatic sync", outdatedLibraryIds.Count);

        var queueResult = await _commandExecutor.Send(
            new QueueLibrarySyncJobCommand(outdatedLibraryIds),
            cancellationToken
        );

        if (queueResult.IsCancelled)
            throw new OperationCanceledException(cancellationToken);

        if (queueResult.IsFailed)
        {
            queueResult.LogError();
            throw new InvalidOperationException(
                $"Failed to queue outdated Plex libraries for automatic sync: {string.Join("; ", queueResult.Errors.Select(x => x.Message))}"
            );
        }
    }

    protected override Task<CheckPlexLibrariesForUpdatesJobUpdate?> GetStatusUpdateDataAsync(
        TickerFunctionContext<CheckPlexLibrariesForUpdatesJobPayload> context,
        CancellationToken cancellationToken
    ) => Task.FromResult<CheckPlexLibrariesForUpdatesJobUpdate?>(new());
}

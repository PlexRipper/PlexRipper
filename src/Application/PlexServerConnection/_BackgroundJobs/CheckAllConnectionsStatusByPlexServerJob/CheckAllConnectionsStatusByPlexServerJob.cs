using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public sealed record CheckAllConnectionsStatusByPlexServerJobPayload;

/// <summary>
/// Checks the status of every connection for every Plex server.
/// </summary>
public class CheckAllConnectionsStatusByPlexServerJob
    : BaseBackgroundJob<CheckAllConnectionsStatusByPlexServerJobPayload, CheckAllConnectionStatusUpdateDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public CheckAllConnectionsStatusByPlexServerJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    ) : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<CheckAllConnectionsStatusByPlexServerJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    protected override JobTypes JobType => JobTypes.CheckAllConnectionsStatusByPlexServerJob;

    protected override List<RefreshDataType> RefreshDataTypes =>
        [RefreshDataType.PlexServer, RefreshDataType.PlexServerConnection];

    public static JobKey GetJobKey() =>
        new(
            nameof(JobTypes.CheckAllConnectionsStatusByPlexServerJob),
            JobTypes.CheckAllConnectionsStatusByPlexServerJob
        );

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<CheckAllConnectionsStatusByPlexServerJobPayload> context,
        CancellationToken cancellationToken
    )
    {      
        context.CronOccurrenceOperations.SkipIfAlreadyRunning();

        var plexServerIds = await _dbContext.PlexServers.Select(x => x.Id).ToListAsync(cancellationToken);
        if (plexServerIds.Count == 0)
            return;

        var connectionResults = await Task.WhenAll(
            plexServerIds.Select(plexServerId =>
                _commandExecutor.Send(
                    new CheckAllConnectionsStatusByPlexServerCommand(plexServerId),
                    cancellationToken
                )
            )
        );

        var cancelledResults = connectionResults.Where(x => x.IsCancelled).ToList();
        foreach (var cancelledResult in cancelledResults)
            cancelledResult.LogWarning();

        if (cancelledResults.Count > 0)
            throw new OperationCanceledException(cancellationToken);

        var failedResults = connectionResults.Where(x => x.IsFailed && !x.IsCancelled).ToList();
        foreach (var failedResult in failedResults)
            failedResult.LogWarning();

        _log.Here()
            .Debug(
                "{JobName} for servers with ids: {PlexServerIds} completed with {FailedServerCount} unavailable servers",
                nameof(CheckAllConnectionsStatusByPlexServerJob),
                plexServerIds,
                failedResults.Count
            );
    }

    protected override async Task<CheckAllConnectionStatusUpdateDTO?> GetStatusUpdateDataAsync(
        TickerFunctionContext<CheckAllConnectionsStatusByPlexServerJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        var plexServers = await _dbContext.PlexServers
            .Include(x => x.PlexServerConnections)
            .ToListAsync(cancellationToken);

        return new CheckAllConnectionStatusUpdateDTO
        {
            PlexServersWithConnectionIds = plexServers.ToDictionary(
                x => x.Id,
                x => x.PlexServerConnections.Select(y => y.Id).ToList()
            ),
        };
    }
}

using TickerQ.Utilities.Base;

namespace Reaparr.Application;

public sealed record InspectPlexServerJobPayload
{
    public required List<int> PlexServerIds { get; init; }
}

/// <summary>
/// Executed on a new Plex Account to check all connections and refresh libraries.
/// </summary>
public class InspectPlexServerJob : BaseBackgroundJob<InspectPlexServerJobPayload, InspectPlexServerJobUpdateDTO>
{
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly INotificationHubService _notificationHubService;
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    protected override JobTypes JobType => JobTypes.InspectPlexServerJob;

    protected override List<RefreshDataType> RefreshDataTypes =>
        [RefreshDataType.PlexServer, RefreshDataType.PlexServerConnection, RefreshDataType.PlexLibrary];

    public static JobKey GetJobKey(int plexServerId) =>
        new($"{nameof(JobTypes.InspectPlexServerJob)}_{plexServerId}", JobTypes.InspectPlexServerJob);

    public InspectPlexServerJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContextFactory dbContextFactory,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    )
        : base(log, progressHubService, notificationHubService)
    {
        _log = log.ForContext<InspectPlexServerJob>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
        _notificationHubService = notificationHubService;
    }

    protected override async Task ExecuteJobAsync(
        TickerFunctionContext<InspectPlexServerJobPayload> context,
        CancellationToken cancellationToken
    )
    {
        var plexServerIds = context.Request.PlexServerIds;

        _log.Here()
            .Debug(
                "Executing job: {InspectPlexServerJobName} for {Count} servers",
                nameof(InspectPlexServerJob),
                plexServerIds.Count
            );

        var serverTasks = plexServerIds.Select(plexServerId => InspectPlexServer(plexServerId, cancellationToken));
        var results = await Task.WhenAll(serverTasks);
        var cancelledResults = results.Where(x => x.IsCancelled).ToList();
        foreach (var cancelledResult in cancelledResults)
            cancelledResult.LogWarning();

        var failedResults = results.Where(x => x.IsFailed && !x.IsCancelled).ToList();
        foreach (var failedResult in failedResults)
            failedResult.LogError();

        if (failedResults.Count == 0 && cancelledResults.Count == 0)
            _log.Here().Information("Successfully finished the inspection of {Count}", plexServerIds.Count);
    }

    protected override Task<InspectPlexServerJobUpdateDTO?> GetStatusUpdateDataAsync(
        TickerFunctionContext<InspectPlexServerJobPayload> context,
        CancellationToken cancellationToken
    ) =>
        Task.FromResult<InspectPlexServerJobUpdateDTO?>(
            new InspectPlexServerJobUpdateDTO { PlexServerIds = context.Request.PlexServerIds }
        );

    private async Task<Result> InspectPlexServer(int plexServerId, CancellationToken cancellationToken)
    {
        // Check all Plex Server Connections
        var checkResult = await _commandExecutor.Send(
            new CheckAllConnectionsStatusByPlexServerCommand(plexServerId, Timeout: 5),
            cancellationToken
        );

        if (checkResult.IsCancelled)
            return checkResult.ToResult();

        if (checkResult.IsFailed)
        {
            checkResult.LogError();
            return checkResult.ToResult();
        }

        using var dbContext = await _dbContextFactory.CreateAsync();
        return await RefreshAndSyncLibraries(dbContext, plexServerId, cancellationToken);
    }

    private async Task<Result> RefreshAndSyncLibraries(
        IReaparrDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken
    )
    {
        // Refresh accessible libraries
        var accountsResult = await dbContext.GetPlexAccountsWithAccessAsync(plexServerId, cancellationToken);
        if (accountsResult.IsCancelled)
            return accountsResult.ToResult();

        if (accountsResult.IsFailed)
            return accountsResult.LogError();

        var plexAccountId = accountsResult.Value.First().Id;
        var refreshResult = await _commandExecutor.Send(
            new RefreshLibraryAccessCommand(plexAccountId, plexServerId),
            cancellationToken
        );
        if (refreshResult.IsCancelled)
            return refreshResult.ToResult();

        if (refreshResult.IsFailed)
            return refreshResult.LogError();

        // Notify front-end
        await _notificationHubService.SendRefreshNotificationAsync([
            RefreshDataType.PlexAccount,
            RefreshDataType.PlexLibrary,
        ]);

        var libraryIds = await dbContext
            .PlexLibraries.Where(x => x.PlexServerId == plexServerId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        // Sync library media
        return await _commandExecutor.Send(new QueueLibrarySyncJobCommand(libraryIds), cancellationToken);
    }
}

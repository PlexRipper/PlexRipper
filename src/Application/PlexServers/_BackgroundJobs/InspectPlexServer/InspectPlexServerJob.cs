namespace Reaparr.Application;

/// <summary>
/// Executed on a new Plex Account to check all connections and refresh libraries.
/// </summary>
public class InspectPlexServerJob : IJob
{
    public static string PlexServerIdsParameter => "plexServerIds";

    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly INotificationHubService _notificationHubService;
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public static JobKey GetJobKey() => new(Guid.NewGuid().ToString(), nameof(InspectPlexServerJob));

    public InspectPlexServerJob(
        ILogger log,
        ICommandExecutor commandExecutor,
        IReaparrDbContextFactory dbContextFactory,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<InspectPlexServerJob>();
        _commandExecutor = commandExecutor;
        _dbContextFactory = dbContextFactory;
        _notificationHubService = notificationHubService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var dataMap = context.JobDetail.JobDataMap;
        var cancellationToken = context.CancellationToken;

        var plexServerIds = dataMap.GetIntListValue(PlexServerIdsParameter);

        _log.Here()
            .Debug(
                "Executing job: {InspectPlexServerJobName} for {Count} servers",
                nameof(InspectPlexServerJob),
                plexServerIds.Count
            );

        var executionResult = await Result.Try(async Task () =>
        {
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
        });

        if (executionResult.IsCancelled)
            executionResult.LogWarning();
        else if (executionResult.IsFailed)
            executionResult.LogError();
    }

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

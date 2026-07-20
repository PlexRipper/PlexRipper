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

        try
        {
            var serverTasks = plexServerIds.Select(plexServerId => InspectPlexServer(plexServerId, cancellationToken));
            await Task.WhenAll(serverTasks);

            _log.Here().Information("Successfully finished the inspection of {Count}", plexServerIds.Count);
        }
        catch (Exception e)
        {
            // Jobs should swallow exceptions as otherwise Quartz will keep re-executing it
            // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
            _log.Here().ErrorResult(e);
        }
    }

    private async Task InspectPlexServer(int plexServerId, CancellationToken cancellationToken)
    {
        // Check all Plex Server Connections
        var checkResult = await _commandExecutor.Send(
            new CheckAllConnectionsStatusByPlexServerCommand(plexServerId),
            cancellationToken
        );

        if (checkResult.IsFailed)
        {
            checkResult.LogError();
            return;
        }

        using var dbContext = await _dbContextFactory.CreateAsync();
        await RefreshAndSyncLibraries(dbContext, plexServerId, cancellationToken);
    }

    private async Task<Result> RefreshAndSyncLibraries(
        IReaparrDbContext dbContext,
        int plexServerId,
        CancellationToken cancellationToken
    )
    {
        // Refresh accessible libraries
        var accountsResult = await dbContext.GetPlexAccountsWithAccessAsync(plexServerId, cancellationToken);
        if (accountsResult.IsFailed)
            return accountsResult.ToResult().LogError();

        var plexAccountId = accountsResult.Value.First().Id;
        await _commandExecutor.Send(new RefreshLibraryAccessCommand(plexAccountId, plexServerId), cancellationToken);

        // Notify front-end
        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexAccount, RefreshDataType.PlexLibrary],
            CancellationToken.None
        );

        var libraryIds = await dbContext
            .PlexLibraries.Where(x => x.PlexServerId == plexServerId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        // Sync library media
        await _commandExecutor.Send(new QueueLibrarySyncJobCommand(libraryIds), cancellationToken);

        return Result.Ok();
    }
}

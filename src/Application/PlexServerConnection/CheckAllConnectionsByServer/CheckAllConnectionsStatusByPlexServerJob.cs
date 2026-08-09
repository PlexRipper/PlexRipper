namespace Reaparr.Application;

/// <summary>
/// This job will check the status of all connections for a given Plex Server and runs periodically.
/// </summary>
[DisallowConcurrentExecution]
public class CheckAllConnectionsStatusByPlexServerJob : IJob
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IProgressHubService _progressHubService;

    public static JobKey GetJobKey() =>
        new(nameof(CheckAllConnectionsStatusByPlexServerJob), nameof(CheckAllConnectionsStatusByPlexServerJob));

    public CheckAllConnectionsStatusByPlexServerJob(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService
    )
    {
        _log = log.ForContext<CheckAllConnectionsStatusByPlexServerJob>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressHubService = progressHubService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var result = await Result.Try(async Task () =>
        {
            var cancellationToken = context.CancellationToken;
            var plexServers = _dbContext.PlexServers
                .Include(x => x.PlexServerConnections)
                .ToList();

            if (!plexServers.Any())
            {
                return;
            }

            // Send start job status update
            var update = new JobStatusUpdate<CheckAllConnectionStatusUpdateDTO>(
                JobTypes.CheckAllConnectionsStatusByPlexServerJob,
                JobStatus.Started,
                new CheckAllConnectionStatusUpdateDTO
                {
                    PlexServersWithConnectionIds = plexServers.ToDictionary(
                        x => x.Id,
                        x => x.PlexServerConnections.Select(y => y.Id).ToList()
                    ),
                }
            );

            await _progressHubService.SendJobStatusUpdateAsync(update);

            var connectionResults = await Task.WhenAll(
                plexServers.Select(async plexServer =>
                    await _commandExecutor.Send(
                        new CheckAllConnectionsStatusByPlexServerCommand(plexServer.Id),
                        cancellationToken
                    )
                )
            );

            var cancelledResults = connectionResults.Where(x => x.IsCancelled).ToList();
            foreach (var cancelledResult in cancelledResults)
                cancelledResult.LogWarning();

            var failedResults = connectionResults.Where(x => x.IsFailed && !x.IsCancelled).ToList();
            foreach (var failedResult in failedResults)
                failedResult.LogError();

            if (cancelledResults.Count > 0 || failedResults.Count > 0)
                return;

            // Send completed job status update
            update.Status = JobStatus.Completed;
            await _progressHubService.SendJobStatusUpdateAsync(update);

            _log.Here()
                .Debug(
                    "{JobName} for servers with ids: {PlexServerIds} completed",
                    nameof(CheckAllConnectionsStatusByPlexServerJob),
                    plexServers.Select(x => x.Id).ToList()
                );
        });

        if (result.IsCancelled)
            result.LogWarning();
        else if (result.IsFailed)
            result.LogError();
    }
}

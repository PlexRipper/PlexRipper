namespace Reaparr.Application;

public class CheckAllPlexServerConnectionsCommandValidator
    : AbstractValidator<CheckAllPlexServerConnectionsCommand>
{
    public CheckAllPlexServerConnectionsCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class CheckAllPlexServerConnectionsCommandHandler
    : ICommandHandler<CheckAllPlexServerConnectionsCommand, Result>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IProgressHubService _progressHubService;

    public CheckAllPlexServerConnectionsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ICommandExecutor commandExecutor,
        IProgressHubService progressHubService
    )
    {
        _log = log.ForContext<CheckAllPlexServerConnectionsCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressHubService = progressHubService;
    }

    public async Task<Result> ExecuteAsync(
        CheckAllPlexServerConnectionsCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexServers = await _dbContext
            .PlexServers.Include(x => x.PlexServerConnections)
            .ToListAsync(cancellationToken);

        if (!plexServers.Any())
            return Result.Ok();

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

        var connectionResults = new List<Result>();
        foreach (var plexServer in plexServers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            connectionResults.Add(
                (
                    await _commandExecutor.Send(
                        new CheckAllConnectionsStatusByPlexServerCommand(plexServer.Id),
                        cancellationToken
                    )
                ).ToResult()
            );
        }

        var cancelledResult = connectionResults.FirstOrDefault(x => x.IsCancelled);
        if (cancelledResult is not null)
            return cancelledResult;

        foreach (var failedResult in connectionResults.Where(x => x.IsFailed))
            failedResult.LogWarning();

        update.Status = JobStatus.Completed;
        await _progressHubService.SendJobStatusUpdateAsync(update);

        _log.Here()
            .Debug(
                "Checked connections for Plex servers with ids: {PlexServerIds}",
                plexServers.Select(x => x.Id).ToList()
            );

        return Result.Ok();
    }
}
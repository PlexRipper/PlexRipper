namespace Reaparr.Application;

/// <summary>
/// This job checks the status of all connections for enabled Plex servers and runs periodically.
/// </summary>
[DisallowConcurrentExecution]
public class CheckAllConnectionsStatusByPlexServerJob : IJob
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public static JobKey GetJobKey() =>
        new(nameof(CheckAllConnectionsStatusByPlexServerJob), nameof(CheckAllConnectionsStatusByPlexServerJob));

    public CheckAllConnectionsStatusByPlexServerJob(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CheckAllConnectionsStatusByPlexServerJob>();
        _commandExecutor = commandExecutor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _log.Here().Debug("Executing job: {JobName}", nameof(CheckAllConnectionsStatusByPlexServerJob));

        var result = await Result.Try(() =>
            _commandExecutor.Send(new CheckAllPlexServerConnectionsCommand(), context.CancellationToken)
        );

        if (result.IsCancelled)
            result.LogWarning();
        else if (result.IsFailed)
            result.LogError();
    }
}

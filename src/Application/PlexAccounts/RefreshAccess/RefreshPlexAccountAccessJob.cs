namespace Reaparr.Application;

/// <summary>
/// Periodically validates enabled Plex accounts and refreshes their server and library access.
/// </summary>
[DisallowConcurrentExecution]
public class RefreshPlexAccountAccessJob : IJob
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public RefreshPlexAccountAccessJob(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<RefreshPlexAccountAccessJob>();
        _commandExecutor = commandExecutor;
    }

    public static JobKey GetJobKey() =>
        new(nameof(JobTypes.RefreshPlexAccountAccessJob), nameof(JobTypes.RefreshPlexAccountAccessJob));

    public async Task Execute(IJobExecutionContext context)
    {
        _log.Here().Debug("Executing job: {JobName}", nameof(RefreshPlexAccountAccessJob));

        var result = await Result.Try(() =>
            _commandExecutor.Send(new RefreshPlexAccountAccessCommand(), context.CancellationToken)
        );

        if (result.IsCancelled)
            result.LogWarning();
        else if (result.IsFailed)
            result.LogError();
    }
}

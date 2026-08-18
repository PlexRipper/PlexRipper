namespace Reaparr.Application;

/// <summary>
/// Periodically validates enabled Plex accounts and refreshes their server and library access.
/// </summary>
public class RefreshPlexAccountAccessJob : IJob
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public RefreshPlexAccountAccessJob(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<RefreshPlexAccountAccessJob>();
        _commandExecutor = commandExecutor;
    }

    protected JobTypes JobType => JobTypes.RefreshPlexAccountAccessJob;

    protected List<RefreshDataType> RefreshDataTypes =>
        [RefreshDataType.PlexAccount, RefreshDataType.PlexServer, RefreshDataType.PlexLibrary];

    public static JobKey GetJobKey() =>
        new(nameof(JobTypes.RefreshPlexAccountAccessJob), nameof(JobTypes.RefreshPlexAccountAccessJob));

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        _log.Here().Debug("Executing job: {JobName}", nameof(RefreshPlexAccountAccessJob));

        var result = await _commandExecutor.Send(new RefreshPlexAccountAccessCommand(), cancellationToken);

        if (result.IsCancelled)
        {
            context.SetResult(JobStatus.Cancelled, result);
            result.LogWarning();
            return;
        }

        if (result.IsFailed)
        {
            context.SetResult(JobStatus.Failed, result);
            result.LogError();
            return;
        }

        _log.Here()
            .Debug(
                "{JobName} refreshed access for {PlexAccountCount} Plex accounts",
                nameof(RefreshPlexAccountAccessJob),
                result.Value.Count
            );
        context.SetResult(JobStatus.Completed);
    }
}

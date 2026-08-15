namespace Reaparr.Application;

public sealed record RefreshPlexAccountAccessJobPayload;

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
        _log.Here().Debug("Executing job: {JobName}", nameof(RefreshPlexAccountAccessJob));

        var cancellationToken = context.CancellationToken;

        var result = await _commandExecutor.Send(new RefreshPlexAccountAccessCommand(), cancellationToken);

        if (result.IsCancelled)
        {
            result.LogWarning();
            throw new OperationCanceledException(cancellationToken);
        }

        if (result.IsFailed)
        {
            result.LogError();
            throw new InvalidOperationException(
                string.Join(System.Environment.NewLine, result.Errors.Select(x => x.Message))
            );
        }

        _log.Here()
            .Debug(
                "{JobName} refreshed access for {PlexAccountCount} Plex accounts",
                nameof(RefreshPlexAccountAccessJob),
                result.Value.Count
            );
    }
}

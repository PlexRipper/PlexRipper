namespace Reaparr.BackgroundJobs;

/// <summary>
/// Periodically checks whether Plex libraries changed and queues full library syncs only when needed.
/// </summary>
[DisallowConcurrentExecution]
public class CheckPlexLibrariesForUpdatesJob : IJob
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;

    public CheckPlexLibrariesForUpdatesJob(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CheckPlexLibrariesForUpdatesJob>();
        _commandExecutor = commandExecutor;
    }

    public static JobKey GetJobKey() =>
        new(nameof(JobTypes.CheckPlexLibrariesForUpdatesJob), nameof(JobTypes.CheckPlexLibrariesForUpdatesJob));

    public async Task Execute(IJobExecutionContext context)
    {
        _log.Here().Debug("Executing job: {JobName}", nameof(CheckPlexLibrariesForUpdatesJob));

        var result = await Result.Try(() =>
            _commandExecutor.Send(new CheckPlexLibrariesForUpdatesCommand(), context.CancellationToken)
        );

        if (result.IsCancelled)
            result.LogWarning();
        else if (result.IsFailed)
            result.LogError();
    }
}

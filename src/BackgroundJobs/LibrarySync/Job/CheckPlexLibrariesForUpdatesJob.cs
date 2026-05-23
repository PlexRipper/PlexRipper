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

        try
        {
            await _commandExecutor.Send(new CheckPlexLibrariesForUpdatesCommand(), context.CancellationToken);
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
        }
    }
}

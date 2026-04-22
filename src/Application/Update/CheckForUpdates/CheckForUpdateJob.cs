namespace Reaparr.Application;

/// <summary>
/// Dispatches <see cref="CheckForUpdatesCommand"/> on a recurring schedule.
/// The command handles update checking and notifies the front-end when an update is found.
/// </summary>
[DisallowConcurrentExecution]
public class CheckForUpdateJob : IJob
{
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger _log;

    public static JobKey GetJobKey() => new(nameof(CheckForUpdateJob), nameof(CheckForUpdateJob));

    public CheckForUpdateJob(ILogger log, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<CheckForUpdateJob>();
        _commandExecutor = commandExecutor;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _log.Here().Debug("Executing job: {JobName}", nameof(CheckForUpdateJob));

        try
        {
            await _commandExecutor.Send(new CheckForUpdatesCommand(), context.CancellationToken);
        }
        catch (Exception e)
        {
            // Jobs must not throw — Quartz would otherwise keep re-executing
            // https://www.quartz-scheduler.net/documentation/best-practices.html#throwing-exceptions
            _log.Here().ErrorResult(e);
        }
    }
}

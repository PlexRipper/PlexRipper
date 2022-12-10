using PlexRipper.Application;
using Quartz;

namespace BackgroundServices;

public class SchedulerService : ISchedulerService
{
    #region Fields

    private readonly IScheduler _scheduler;

    #endregion

    #region Constructors

    public SchedulerService(IScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    #endregion

    #region Methods

    #region Public

    /// <summary>
    /// Will start the <see cref="IScheduler"/> of Quartz for all the background services.
    /// </summary>
    /// <returns></returns>
    public async Task<Result> SetupAsync()
    {
        if (!_scheduler.IsStarted)
        {
            Log.Information("Starting Quartz Scheduler");
            await _scheduler.Start();
        }

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail($"Could not start Scheduler {_scheduler.SchedulerName}").LogError();
    }

    public async Task<Result> StopAsync(bool graceFully = true)
    {
        if (!_scheduler.IsShutdown)
            await _scheduler.Shutdown(graceFully);

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not shutdown Scheduler").LogError();
    }

    #endregion

    #endregion
}
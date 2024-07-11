using Application.Contracts;
using Logging.Interface;
using Quartz;
using Quartz.Impl.Matchers;

namespace PlexRipper.Application;

public class SchedulerService : ISchedulerService
{
    #region Fields

    private readonly ILog<SchedulerService> _log;
    private readonly IScheduler _scheduler;
    private readonly IAllJobListener _allJobListener;

    #endregion

    #region Constructors

    public SchedulerService(ILog<SchedulerService> log, IScheduler scheduler, IAllJobListener allJobListener)
    {
        _log = log;
        _scheduler = scheduler;
        _allJobListener = allJobListener;
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
        SetupListeners();
        if (!_scheduler.IsStarted)
        {
            _log.InformationLine("Starting Quartz Scheduler");
            await _scheduler.Start();
        }

        return _scheduler.IsStarted
            ? Result.Ok()
            : Result.Fail($"Could not start Scheduler {_scheduler.SchedulerName}").LogError();
    }

    public async Task<Result> StopAsync(bool graceFully = true)
    {
        if (!_scheduler.IsShutdown)
            await _scheduler.Shutdown(graceFully).WaitAsync(TimeSpan.FromSeconds(15));

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not shutdown Scheduler").LogError();
    }

    #endregion

    private void SetupListeners()
    {
        _log.DebugLine("Setting up Quartz listeners");
        _scheduler.ListenerManager.AddJobListener(_allJobListener, GroupMatcher<JobKey>.AnyGroup());
    }

    public async Task AwaitScheduler(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken);
        var isExecutingJobs = true;
        while (isExecutingJobs)
        {
            await Task.Delay(1000, cancellationToken);
            var executingJobs = await _scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            isExecutingJobs = executingJobs.Count > 0;
            _log.Verbose("Currently number of executing jobs: {ExecutingJobsCount}", executingJobs.Count);
        }

        await Task.Delay(1000, cancellationToken);
    }

    #endregion
}

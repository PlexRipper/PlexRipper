using Quartz;
using Quartz.Impl.Matchers;
using Reaparr.Application.Contracts;
using Reaparr.Environment;
using Serilog;

namespace Reaparr.Application;

public class SchedulerService : ISchedulerService
{
    #region Fields

    private readonly Serilog.ILogger _log;
    private readonly IScheduler _scheduler;
    private readonly IAllJobListener _allJobListener;
    private readonly IDownloadJobListener _downloadJobListener;

    #endregion

    #region Constructors

    public SchedulerService(
        ILogger log,
        IScheduler scheduler,
        IAllJobListener allJobListener,
        IDownloadJobListener downloadJobListener
    )
    {
        _log = log.ForContext<SchedulerService>();
        _scheduler = scheduler;
        _allJobListener = allJobListener;
        _downloadJobListener = downloadJobListener;
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
            _log.Debug("Starting Quartz Scheduler");
            await _scheduler.Start();
        }

        if (!EnvironmentExtensions.IsIntegrationTestMode())
        {
            await SetupPlexServerStatusCheckJob(CancellationToken.None);
        }

        return _scheduler.IsStarted
            ? Result.Ok()
            : Result.Fail($"Could not start Scheduler {_scheduler.SchedulerName}").LogError();
    }

    public async Task<Result> StopAsync()
    {
        if (!_scheduler.IsShutdown)
        {
            _log.Debug("Shutting down Quartz Scheduler");

            foreach (var runningJob in await _scheduler.GetCurrentlyExecutingJobs())
            {
                _log.Warning("Stopping running job {JobKey}", runningJob.JobDetail.Key.ToString());
                await _scheduler.Interrupt(runningJob.JobDetail.Key);
            }

            // Jobs can be interrupted and later resume from where the left off
            await _scheduler.Shutdown(true).WaitAsync(TimeSpan.FromSeconds(15));
        }

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not shutdown Scheduler").LogError();
    }

    #endregion

    private void SetupListeners()
    {
        _log.Debug("Setting up Quartz listeners");
        _scheduler.ListenerManager.AddJobListener(_allJobListener, GroupMatcher<JobKey>.AnyGroup());
        _scheduler.ListenerManager.AddJobListener(
            _downloadJobListener,
            GroupMatcher<JobKey>.GroupEquals(DownloadJob.GetJobKey(Guid.Empty).Group)
        );
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

    public async Task SetupPlexServerStatusCheckJob(CancellationToken cancellationToken)
    {
        var key = CheckAllConnectionsStatusByPlexServerJob.GetJobKey();

        if (await _scheduler.CheckExists(key, CancellationToken.None))
        {
            return;
        }

        var job = JobBuilder.Create<CheckAllConnectionsStatusByPlexServerJob>().WithIdentity(key).Build();

        var trigger = TriggerBuilder
            .Create()
            .WithIdentity($"{key.Name}_trigger", key.Group)
            .ForJob(job)
            .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever())
            .Build();

        await _scheduler.ScheduleJob(job, trigger, cancellationToken);
    }

    public async Task<List<JobStatusUpdate<string>>> GetRunningJobUpdates() =>
        (await _scheduler.GetCurrentlyExecutingJobs()).Select(x => x.ToJobStatusUpdate(JobStatus.Started)).ToList();

    #endregion
}

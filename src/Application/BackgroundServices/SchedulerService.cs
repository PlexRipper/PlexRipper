using Quartz.Impl.Matchers;

namespace Reaparr.Application;

public class SchedulerService : ISchedulerService
{
    #region Fields

    private readonly ILogger _log;
    private readonly IScheduler _scheduler;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IAllJobListener _allJobListener;
    private readonly IDownloadJobListener _downloadJobListener;

    #endregion

    #region Constructors

    public SchedulerService(
        ILogger log,
        IScheduler scheduler,
        IAppRuntimeInfo appRuntimeInfo,
        ICommandExecutor commandExecutor,
        IAllJobListener allJobListener,
        IDownloadJobListener downloadJobListener
    )
    {
        _log = log.ForContext<SchedulerService>();
        _scheduler = scheduler;
        _appRuntimeInfo = appRuntimeInfo;
        _commandExecutor = commandExecutor;
        _allJobListener = allJobListener;
        _downloadJobListener = downloadJobListener;
    }

    #endregion

    #region Methods

    #region Public

    /// <summary>
    /// Registers scheduler-owned listeners, recreates recurring schedules, and starts Quartz.
    /// </summary>
    public async Task<Result> SetupAsync(CancellationToken cancellationToken = default)
    {
        SetupListeners();

        if (!_appRuntimeInfo.IsIntegrationTestMode)
        {
            var setupRefreshPlexAccountAccessResult = await SetupRefreshPlexAccountAccessJob(cancellationToken);
            setupRefreshPlexAccountAccessResult.LogIfFailed();

            var setupPlexServerStatusCheckResult = await SetupPlexServerStatusCheckJob(cancellationToken);
            setupPlexServerStatusCheckResult.LogIfFailed();

            var setupUpdateCheckResult = await SetupUpdateCheckJob(cancellationToken);
            setupUpdateCheckResult.LogIfFailed();

            var setupLibrarySyncResult = await SetupLibrarySyncJob(cancellationToken);
            if (setupLibrarySyncResult.IsCancelled)
                return setupLibrarySyncResult;

            setupLibrarySyncResult.LogIfFailed();

            var setupLibraryComparisonResult = await SetupLibraryComparisonJob(cancellationToken);
            if (setupLibraryComparisonResult.IsCancelled)
                return setupLibraryComparisonResult;

            setupLibraryComparisonResult.LogIfFailed();
        }

        if (!_scheduler.IsStarted)
        {
            _log.Here().Debug("Starting Quartz Scheduler");
            await _scheduler.Start(cancellationToken);
        }

        return _scheduler.IsStarted
            ? Result.Ok()
            : Result.Fail($"Could not start Scheduler {_scheduler.SchedulerName}").LogError();
    }

    public async Task<Result> StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_scheduler.IsShutdown)
        {
            _log.Here().Debug("Shutting down Quartz Scheduler");

            foreach (var runningJob in await _scheduler.GetCurrentlyExecutingJobs(cancellationToken))
            {
                _log.Here().Warning("Stopping running job {JobKey}", runningJob.JobDetail.Key.ToString());
                await _scheduler.Interrupt(runningJob.JobDetail.Key, cancellationToken);
            }

            // Jobs can be interrupted and later resume from where they left off
            await _scheduler
                .Shutdown(true, cancellationToken)
                .WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
        }

        return _scheduler.IsStarted ? Result.Ok() : Result.Fail("Could not shutdown Scheduler").LogError();
    }

    #endregion

    private void SetupListeners()
    {
        _log.Here().Debug("Setting up Quartz listeners");
        _scheduler.ListenerManager.AddJobListener(_allJobListener, GroupMatcher<JobKey>.AnyGroup());
        _scheduler.ListenerManager.AddJobListener(
            _downloadJobListener,
            GroupMatcher<JobKey>.GroupEquals(DownloadJob.GetJobKey(Guid.Empty).Group)
        );
    }

    private async Task<Result> SetupRefreshPlexAccountAccessJob(CancellationToken cancellationToken)
        {
            var key = RefreshPlexAccountAccessJob.GetJobKey();
            if (await _scheduler.CheckExists(key, cancellationToken))
                await _scheduler.DeleteJob(key, cancellationToken);

            var job = JobBuilder.Create<RefreshPlexAccountAccessJob>().WithIdentity(key).Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity($"{key.Name}_trigger", key.Group)
                .ForJob(job)
                .StartAt(DateTimeOffset.UtcNow.AddHours(6))
                .WithSimpleSchedule(x => x.WithIntervalInHours(6).RepeatForever())
                .Build();

        return await Result.Try(async Task () => await _scheduler.ScheduleJob(job, trigger, cancellationToken));
    }

    private async Task<Result> SetupPlexServerStatusCheckJob(CancellationToken cancellationToken)
        {
            var key = CheckAllConnectionsStatusByPlexServerJob.GetJobKey();
            if (await _scheduler.CheckExists(key, cancellationToken))
                await _scheduler.DeleteJob(key, cancellationToken);

            var job = JobBuilder.Create<CheckAllConnectionsStatusByPlexServerJob>().WithIdentity(key).Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity($"{key.Name}_trigger", key.Group)
                .ForJob(job)
                .StartAt(DateTimeOffset.UtcNow.AddMinutes(10))
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(10).RepeatForever())
                .Build();

        return await Result.Try(async Task () => await _scheduler.ScheduleJob(job, trigger, cancellationToken));
    }

    private async Task<Result> SetupUpdateCheckJob(CancellationToken cancellationToken)
        {
            var key = CheckForUpdateJob.GetJobKey();
            if (await _scheduler.CheckExists(key, cancellationToken))
                await _scheduler.DeleteJob(key, cancellationToken);

            var job = JobBuilder.Create<CheckForUpdateJob>().WithIdentity(key).Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity($"{key.Name}_trigger", key.Group)
                .ForJob(job)
                .StartAt(DateTimeOffset.UtcNow.AddHours(1))
                .WithSimpleSchedule(x => x.WithIntervalInHours(1).RepeatForever())
                .Build();

        return await Result.Try(async Task () => await _scheduler.ScheduleJob(job, trigger, cancellationToken));
    }

    private Task<Result> SetupLibrarySyncJob(CancellationToken cancellationToken) =>
        _commandExecutor.Send(new QueueCheckPlexLibraryUpdatesJobCommand(), cancellationToken);

    public async Task AwaitScheduler(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken);
        var isExecutingJobs = true;
        while (isExecutingJobs)
        {
            await Task.Delay(1000, cancellationToken);
            var executingJobs = await _scheduler.GetCurrentlyExecutingJobs(cancellationToken);
            isExecutingJobs = executingJobs.Count > 0;
            _log.Here().Verbose("Currently number of executing jobs: {ExecutingJobsCount}", executingJobs.Count);
        }

        await Task.Delay(1000, cancellationToken);
    }

    private async Task<Result> SetupLibraryComparisonJob(CancellationToken cancellationToken)
    {
        var checkQueuedResult =
            await _commandExecutor.Send(new CheckQueuedLibraryComparisonJobCommand(), cancellationToken);
        if (checkQueuedResult.IsCancelled)
            return checkQueuedResult;

        return checkQueuedResult.IsFailed ? checkQueuedResult.LogError() : Result.Ok();
    }

    public async Task<List<JobStatusUpdate<string>>> GetRunningJobUpdates() =>
        (await _scheduler.GetCurrentlyExecutingJobs()).Select(x => x.ToJobStatusUpdate(JobStatus.Started)).ToList();

    #endregion
}
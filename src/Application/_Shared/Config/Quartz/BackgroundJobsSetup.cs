using Quartz.Impl.Matchers;

namespace Reaparr.Application;

public sealed class BackgroundJobsSetup : IBackgroundJobsSetup
{
    private readonly IScheduler _scheduler;
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly AllJobListener _allJobListener;
    private readonly DownloadJobListener _downloadJobListener;
    private readonly LibrarySyncJobListener _librarySyncJobListener;
    private readonly ReaparrSchedulerListener _schedulerListener;

    public BackgroundJobsSetup(
        IScheduler scheduler,
        IAppRuntimeInfo appRuntimeInfo,
        AllJobListener allJobListener,
        DownloadJobListener downloadJobListener,
        LibrarySyncJobListener librarySyncJobListener,
        ReaparrSchedulerListener schedulerListener
    )
    {
        _scheduler = scheduler;
        _appRuntimeInfo = appRuntimeInfo;
        _allJobListener = allJobListener;
        _downloadJobListener = downloadJobListener;
        _librarySyncJobListener = librarySyncJobListener;
        _schedulerListener = schedulerListener;
    }

    public async Task<Result> SetupAsync(CancellationToken cancellationToken = default)
    {
        return await Result.Try(async Task () =>
        {
            RegisterListeners();

            if (!_appRuntimeInfo.IsIntegrationTestMode)
                await SetupRecurringJobs(cancellationToken);

            await _scheduler.Start(cancellationToken);
        });
    }

    private void RegisterListeners()
    {
        _scheduler.ListenerManager.AddJobListener(_allJobListener, EverythingMatcher<JobKey>.AllJobs());
        _scheduler.ListenerManager.AddJobListener(
            _downloadJobListener,
            GroupMatcher<JobKey>.GroupEquals(nameof(JobTypes.DownloadJob))
        );
        _scheduler.ListenerManager.AddJobListener(
            _librarySyncJobListener,
            GroupMatcher<JobKey>.GroupEquals(nameof(JobTypes.LibrarySyncJob))
        );
        _scheduler.ListenerManager.AddSchedulerListener(_schedulerListener);
    }

    private async Task SetupRecurringJobs(CancellationToken cancellationToken)
    {
        await _scheduler.DeleteJob(CheckAllConnectionsStatusByPlexServerJob.GetJobKey(), cancellationToken);
        await _scheduler.DeleteJob(CheckPlexLibrariesForUpdatesJob.GetJobKey(), cancellationToken);
        await _scheduler.DeleteJob(RefreshPlexAccountAccessJob.GetJobKey(), cancellationToken);
        await _scheduler.DeleteJob(CheckForUpdateJob.GetJobKey(), cancellationToken);

        {
            var jobKey = CheckAllConnectionsStatusByPlexServerJob.GetJobKey();
            var job = JobBuilder
                .Create<CheckAllConnectionsStatusByPlexServerJob>()
                .WithIdentity(jobKey)
                .DisallowConcurrentExecution()
                .StoreDurably()
                .RequestRecovery()
                .Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(jobKey.Name, jobKey.Group)
                .ForJob(jobKey)
                .WithCronSchedule(
                    "0 0/10 * * * ?",
                    x => x.InTimeZone(TimeZoneInfo.Utc).WithMisfireHandlingInstructionDoNothing()
                )
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        {
            var jobKey = CheckPlexLibrariesForUpdatesJob.GetJobKey();
            var job = JobBuilder
                .Create<CheckPlexLibrariesForUpdatesJob>()
                .WithIdentity(jobKey)
                .DisallowConcurrentExecution()
                .StoreDurably()
                .RequestRecovery()
                .Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(jobKey.Name, jobKey.Group)
                .ForJob(jobKey)
                .WithCronSchedule(
                    "0 0 0/3 * * ?",
                    x => x.InTimeZone(TimeZoneInfo.Utc).WithMisfireHandlingInstructionDoNothing()
                )
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        {
            var jobKey = RefreshPlexAccountAccessJob.GetJobKey();
            var job = JobBuilder
                .Create<RefreshPlexAccountAccessJob>()
                .WithIdentity(jobKey)
                .DisallowConcurrentExecution()
                .StoreDurably()
                .RequestRecovery()
                .Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(jobKey.Name, jobKey.Group)
                .ForJob(jobKey)
                .WithCronSchedule(
                    "0 0 0/6 * * ?",
                    x => x.InTimeZone(TimeZoneInfo.Utc).WithMisfireHandlingInstructionDoNothing()
                )
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }

        {
            var jobKey = CheckForUpdateJob.GetJobKey();
            var job = JobBuilder
                .Create<CheckForUpdateJob>()
                .WithIdentity(jobKey)
                .DisallowConcurrentExecution()
                .StoreDurably()
                .RequestRecovery()
                .Build();
            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(jobKey.Name, jobKey.Group)
                .ForJob(jobKey)
                .WithCronSchedule(
                    "0 0 * * * ?",
                    x => x.InTimeZone(TimeZoneInfo.Utc).WithMisfireHandlingInstructionDoNothing()
                )
                .Build();

            await _scheduler.ScheduleJob(job, trigger, cancellationToken);
        }
    }
}

using Quartz.Impl.Matchers;

namespace Reaparr.BackgroundJobs;

public class LibrarySyncJobListener : ILibrarySyncJobListener
{
    private readonly IReaparrDbContextFactory _dbContextFactory;
    private readonly IScheduler _scheduler;
    private readonly IProgressHubService _progressHubService;
    private readonly INotificationHubService _notificationHubService;
    private readonly ILogger _log;

    /// <inheritdoc/>
    public string Name => nameof(LibrarySyncJobListener);

    public LibrarySyncJobListener(
        ILogger log,
        IReaparrDbContextFactory dbContextFactory,
        IScheduler scheduler,
        IProgressHubService progressHubService,
        INotificationHubService notificationHubService
    )
    {
        _log = log.ForContext<LibrarySyncJobListener>();
        _dbContextFactory = dbContextFactory;
        _scheduler = scheduler;
        _progressHubService = progressHubService;
        _notificationHubService = notificationHubService;
    }

    public Result Setup()
    {
        _scheduler.ListenerManager.AddJobListener(
            this,
            GroupMatcher<JobKey>.GroupEquals(nameof(JobTypes.LibrarySyncJob))
        );

        return Result.Ok();
    }

    /// <inheritdoc/>
    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        var result = await Result.Try(async Task () =>
        {
            _log.Here().Debug("JobToBeExecuted for job: {JobKey}", context.JobDetail.Key.ToString());

            await SendStatusUpdate(context, JobStatus.Started, cancellationToken);
        });

        if (result.IsFailed && !result.IsCancelled)
        {
            _log.Here()
                .Error(
                    "Failed to check the {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );

            result.LogIfFailed();
        }
    }

    /// <inheritdoc/>
    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default
    )
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        var result = await Result.Try(async Task () =>
        {
            _log.Here().Debug("JobWasExecuted for job: {JobKey}", context.JobDetail.Key.ToString());

            await SendStatusUpdate(context, JobStatus.Completed, cancellationToken);
        });

        if (result.IsFailed && !result.IsCancelled)
        {
            _log.Here()
                .Error(
                    "Failed to check the {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );

            result.LogIfFailed();
        }
    }

    /// <inheritdoc/>
    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    private async Task SendStatusUpdate(
        IJobExecutionContext context,
        JobStatus jobStatus,
        CancellationToken cancellationToken
    )
    {
        var serverId = context.JobDetail.JobDataMap.GetInt(LibrarySyncJob.ServerIdParameter);
        var libraryId = context.JobDetail.JobDataMap.GetInt(LibrarySyncJob.LibraryIdParameter);

        using var dbContext = await _dbContextFactory.CreateAsync();
        var queue = await dbContext.LibrarySyncJobQueues.FirstOrDefaultAsync(
            x => x.PlexServerId == serverId && x.PlexLibraryId == libraryId,
            CancellationToken.None
        );

        if (queue == null)
        {
            _log.Here()
                .Warning(
                    "Queue item not found for server {ServerId}, library {LibraryId} when sending status update",
                    serverId,
                    libraryId
                );
            return;
        }

        var statusUpdate = new JobStatusUpdate<LibrarySyncJobQueueDTO>(
            JobTypes.LibrarySyncJob,
            jobStatus,
            queue.ToDTO(),
            context.FireInstanceId,
            context.FireTimeUtc.UtcDateTime
        );

        await _progressHubService.SendJobStatusUpdateAsync(statusUpdate);

        await _notificationHubService.SendRefreshNotificationAsync(
            [RefreshDataType.PlexLibrary, RefreshDataType.PlexLibrarySyncStatus]
        );
    }
}
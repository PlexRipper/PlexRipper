using Microsoft.EntityFrameworkCore;
using Quartz;
using Quartz.Impl.Matchers;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public class LibrarySyncJobListener : ILibrarySyncJobListener
{
    private readonly IReaparrDbContext _dbContext;
    private readonly IScheduler _scheduler;
    private readonly ISignalRService _signalRService;
    private readonly ILogger _log;

    /// <inheritdoc/>
    public string Name => nameof(LibrarySyncJobListener);

    public LibrarySyncJobListener(
        ILogger log,
        IReaparrDbContext dbContext,
        IScheduler scheduler,
        ISignalRService signalRService
    )
    {
        _log = log.ForContext<LibrarySyncJobListener>();
        _dbContext = dbContext;
        _scheduler = scheduler;
        _signalRService = signalRService;
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
    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new())
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            _log.Here().Debug("JobToBeExecuted for job: {JobKey}", context.JobDetail.Key.ToString());

            await SendStatusUpdate(context, JobStatus.Started, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(
                    ex,
                    "Failed to check the {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );
        }
    }

    /// <inheritdoc/>
    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = new()
    )
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            _log.Here().Debug("JobWasExecuted for job: {JobKey}", context.JobDetail.Key.ToString());

            await SendStatusUpdate(context, JobStatus.Completed, cancellationToken);
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(
                    ex,
                    "Failed to check the {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );
        }
    }

    /// <inheritdoc/>
    public async Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        await Task.CompletedTask;

    private async Task SendStatusUpdate(
        IJobExecutionContext context,
        JobStatus jobStatus,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var serverId = context.JobDetail.JobDataMap.GetInt(LibrarySyncJob.ServerIdParameter);
            var libraryId = context.JobDetail.JobDataMap.GetInt(LibrarySyncJob.LibraryIdParameter);

            var queue = await _dbContext.LibrarySyncJobQueues.FirstOrDefaultAsync(
                x => x.PlexServerId == serverId && x.PlexLibraryId == libraryId,
                cancellationToken: cancellationToken
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

            await _signalRService.SendJobStatusUpdateAsync(statusUpdate);
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(
                    ex,
                    "Failed to send status update for job {JobKey} with status {JobStatus}",
                    context.JobDetail.Key.ToString(),
                    jobStatus
                );
        }
    }
}

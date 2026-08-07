namespace Reaparr.Application;

public class AllJobListener : IAllJobListener
{
    private readonly IProgressHubService _progressHubService;
    private readonly ILogger _log;

    public string Name => nameof(AllJobListener);

    public AllJobListener(ILogger log, IProgressHubService progressHubService)
    {
        _log = log.ForContext<AllJobListener>();

        _progressHubService = progressHubService;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        var result = await Result.Try(async Task () =>
            await SendJobExecutionContextAsync(context, JobStatus.Started, cancellationToken));

        if (result.IsFailed && !result.IsCancelled)
        {
            _log.Here()
                .Error(
                    "Failed to check the JobToBeExecuted: {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );

            result.LogIfFailed();
        }
    }

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException? jobException,
        CancellationToken cancellationToken = default
    )
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        var result = await Result.Try(async Task () =>
            await SendJobExecutionContextAsync(context, JobStatus.Completed, cancellationToken));

        if (result.IsFailed && !result.IsCancelled)
        {
            _log.Here()
                .Error("Failed to check the JobWasExecuted: {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );

            result.LogIfFailed();
        }
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public async Task SendJobExecutionContextAsync(
        IJobExecutionContext context,
        JobStatus status,
        CancellationToken cancellationToken = default
    )
    {
        // TODO make this consistent and not hacky for certain jobs
        var statusUpdate = context.ToJobStatusUpdate(status);

        if (
            statusUpdate.JobType
            is JobTypes.MoveDownloadFileJob
            or JobTypes.DownloadJob
            or JobTypes.InspectPlexServerJob
        )
        {
            await _progressHubService.SendJobStatusUpdateAsync(statusUpdate);
        }
    }
}
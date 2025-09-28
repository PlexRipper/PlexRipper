using Quartz;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public class AllJobListener : IAllJobListener
{
    private readonly ISignalRService _signalRService;
    private readonly ILogger _log;

    public string Name => nameof(AllJobListener);

    public AllJobListener(ILogger log, ISignalRService signalRService)
    {
        _log = log.ForContext<AllJobListener>();

        _signalRService = signalRService;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new())
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            await SendJobExecutionContextAsync(context, JobStatus.Started);
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(
                    ex,
                    "Failed to check the JobToBeExecuted: {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );
        }
    }

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
            await SendJobExecutionContextAsync(context, JobStatus.Completed);
        }
        catch (Exception ex)
        {
            _log.Here()
                .Error(
                    ex,
                    "Failed to check the JobWasExecuted: {Name} queue after a job was executed: {JobDetail}",
                    Name,
                    context.JobDetail
                );
        }
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;

    public async Task SendJobExecutionContextAsync(IJobExecutionContext context, JobStatus status)
    {
        var statusUpdate = context.ToJobStatusUpdate(status);

        if (statusUpdate.JobType != JobTypes.CheckAllConnectionsStatusByPlexServerJob)
        {
            await _signalRService.SendJobStatusUpdateAsync(statusUpdate);
        }
    }
}

using Application.Contracts;
using Quartz;

namespace PlexRipper.Application;

public class AllJobListener : IAllJobListener
{
    private readonly ISignalRService _signalRService;

    public string Name { get; } = nameof(AllJobListener);

    public AllJobListener(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new())
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            var update = context.ToUpdate(JobStatus.Started);
            await _signalRService.SendJobStatusUpdateAsync(update);
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;

    public async Task JobWasExecuted(
        IJobExecutionContext context,
        JobExecutionException jobException,
        CancellationToken cancellationToken = new()
    )
    {
        // Source: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/trigger-and-job-listeners.html
        // Make sure your trigger and job listeners never throw an exception (use a try-catch) and that they can handle internal problems. Jobs can get stuck after Quartz is unable to determine whether required logic in listener was completed successfully when listener notification failed.
        try
        {
            var update = context.ToUpdate(JobStatus.Completed);
            await _signalRService.SendJobStatusUpdateAsync(update);
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogError();
        }
    }
}

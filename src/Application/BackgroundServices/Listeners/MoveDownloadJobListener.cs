using Quartz;
using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public class MoveDownloadJobListener : IMoveDownloadJobListener
{
    private readonly ILogger _log;
    private readonly IMoveDownloadFileQueue _moveDownloadFileQueue;

    public MoveDownloadJobListener(ILogger log, IMoveDownloadFileQueue moveDownloadFileQueue)
    {
        _log = log.ForContext<MoveDownloadJobListener>();
        _moveDownloadFileQueue = moveDownloadFileQueue;
    }

    public string Name => nameof(MoveDownloadJobListener);

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
            await _moveDownloadFileQueue.CheckMoveDownloadFileJobQueue();
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

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;
}

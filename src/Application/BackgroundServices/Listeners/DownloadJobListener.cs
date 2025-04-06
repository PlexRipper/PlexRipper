using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using Quartz;

namespace PlexRipper.Application;

public class DownloadJobListener : IDownloadJobListener
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IFileMergeQueue _fileMergeQueue;

    public DownloadJobListener(
        ILog log,
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IFileMergeQueue fileMergeQueue
    )
    {
        _log = log;
        _dbContext = dbContext;
        _mediator = mediator;
        _fileMergeQueue = fileMergeQueue;
    }

    public string Name => nameof(DownloadJobListener);

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
            _log.Debug("JobWasExecuted for job: {JobKey}", context.JobDetail.Key.ToString());
            var dataMap = context.JobDetail.JobDataMap;
            var downloadTaskKey = dataMap.GetJsonValue<DownloadTaskKey>(DownloadJob.DownloadTaskIdParameter);
            if (downloadTaskKey is null)
            {
                _log.Error("DownloadTaskKey is null in job: {JobKey}", context.JobDetail.Key.ToString());
                return;
            }

            var status = await _dbContext.GetDownloadTaskStatusAsync(downloadTaskKey, cancellationToken);
            if (status == DownloadStatus.DownloadFinished)
            {
                _log.Debug(
                    "DownloadTask with id: {DownloadTaskId} has finished downloading, starting fileMergeJob and executing DownloadQueueCheck",
                    downloadTaskKey.Id
                );
                await _fileMergeQueue.CheckFileMergeQueue();
                await _mediator.Publish(
                    new CheckDownloadQueueNotification(downloadTaskKey.PlexServerId),
                    cancellationToken
                );
            }
        }
        catch (Exception e)
        {
            Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    public Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = new()) =>
        Task.CompletedTask;
}

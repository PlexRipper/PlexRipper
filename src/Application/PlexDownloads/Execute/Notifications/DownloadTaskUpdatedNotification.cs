using Data.Contracts;
using FileSystem.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public record DownloadTaskUpdatedNotification(DownloadTaskKey Key) : IRequest;

public class DownloadTaskUpdatedHandler : IRequestHandler<DownloadTaskUpdatedNotification>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public DownloadTaskUpdatedHandler(
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        ISignalRService signalRService,
        IFileMergeScheduler fileMergeScheduler)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _signalRService = signalRService;
        _fileMergeScheduler = fileMergeScheduler;
    }

    public async Task Handle(DownloadTaskUpdatedNotification notification, CancellationToken cancellationToken)
    {
        var plexServerId = notification.Key.PlexServerId;

        await _dbContext.DetermineDownloadStatus(notification.Key, cancellationToken);

        var downloadTasks = await _dbContext.GetAllDownloadTasksAsync(plexServerId, cancellationToken: cancellationToken);

        // Send away the new result
        await _signalRService.SendDownloadProgressUpdateAsync(plexServerId, downloadTasks, cancellationToken);

        var downloadTask = await _dbContext.GetDownloadTaskAsync(notification.Key, cancellationToken);
        if (downloadTask.DownloadStatus == DownloadStatus.DownloadFinished)
        {
            var addFileTaskResult = await _fileMergeScheduler.CreateFileTaskFromDownloadTask(notification.Key);
            if (addFileTaskResult.IsFailed)
            {
                addFileTaskResult.LogError();
                return;
            }

            await _fileMergeScheduler.StartFileMergeJob(addFileTaskResult.Value.Id);
            await _mediator.Publish(new CheckDownloadQueueNotification(notification.Key.PlexServerId), cancellationToken);
        }
    }
}
using FileSystem.Contracts;

namespace PlexRipper.Application;

public record DownloadTaskFinishedNotification(DownloadTaskKey Key) : INotification;

public class DownloadTaskFinishedHandler : INotificationHandler<DownloadTaskFinishedNotification>
{
    private readonly IMediator _mediator;
    private readonly IFileMergeScheduler _fileMergeScheduler;

    public DownloadTaskFinishedHandler(IMediator mediator, IFileMergeScheduler fileMergeScheduler)
    {
        _mediator = mediator;
        _fileMergeScheduler = fileMergeScheduler;
    }

    public async Task Handle(DownloadTaskFinishedNotification notification, CancellationToken cancellationToken)
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
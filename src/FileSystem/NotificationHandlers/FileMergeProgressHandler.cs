using Data.Contracts;
using DownloadManager.Contracts;

namespace PlexRipper.FileSystem;

public class FileMergeProgressHandler : INotificationHandler<FileMergeProgressNotification>
{
    private readonly IMediator _mediator;

    public FileMergeProgressHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(FileMergeProgressNotification notification, CancellationToken cancellationToken)
    {
        var downloadTaskResult = await _mediator.Send(new UpdateDownloadTaskWithFileMergeProgressByIdCommand(notification.Progress), cancellationToken);

        await _mediator.Publish(new DownloadTaskUpdated(downloadTaskResult.Value), cancellationToken);
    }
}
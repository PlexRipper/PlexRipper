using Data.Contracts;
using DownloadManager.Contracts;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.FileSystem;

public class FileMergeFinishedHandler : INotificationHandler<FileMergeFinishedNotification>
{
    private readonly IMediator _mediator;
    private readonly IFileMergeSystem _fileMergeSystem;

    public FileMergeFinishedHandler(IMediator mediator, IFileMergeSystem fileMergeSystem)
    {
        _mediator = mediator;
        _fileMergeSystem = fileMergeSystem;
    }

    public async Task Handle(FileMergeFinishedNotification notification, CancellationToken cancellationToken)
    {
        var fileTaskId = notification.FileTaskId;
        var fileTaskResult = await _mediator.Send(new GetFileTaskByIdQuery(fileTaskId), cancellationToken);
        if (fileTaskResult.IsFailed)
        {
            fileTaskResult.LogError();
            return;
        }

        var fileTask = fileTaskResult.Value;

        _fileMergeSystem.DeleteDirectoryFromFilePath(fileTask.FilePaths.First());

        await _mediator.Send(new UpdateDownloadStatusOfDownloadTaskCommand(fileTask.DownloadTaskId, DownloadStatus.Completed), cancellationToken);

        await _mediator.Send(new DeleteFileTaskByIdCommand(notification.FileTaskId), cancellationToken);

        await _mediator.Publish(new DownloadTaskUpdated(fileTask.DownloadTask), cancellationToken);
    }
}
using Data.Contracts;
using DownloadManager.Contracts;
using PlexRipper.FileSystem.Common;

namespace PlexRipper.FileSystem;

public class FileMergeFinishedHandler : INotificationHandler<FileMergeFinishedNotification>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IFileMergeSystem _fileMergeSystem;

    public FileMergeFinishedHandler(IPlexRipperDbContext dbContext, IMediator mediator, IFileMergeSystem fileMergeSystem)
    {
        _dbContext = dbContext;
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
        var downloadTask = await _dbContext.GetDownloadTaskAsync(fileTask.DownloadTaskId, cancellationToken: cancellationToken);
        if (downloadTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(DownloadTask), fileTask.DownloadTaskId).LogError();
            return;
        }

        _fileMergeSystem.DeleteDirectoryFromFilePath(fileTask.FilePaths.First());

        await _dbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Completed, cancellationToken);

        await _mediator.Send(new DeleteFileTaskByIdCommand(notification.FileTaskId), cancellationToken);

        await _mediator.Send(new DownloadTaskUpdated(downloadTask.ToKey()), cancellationToken);
    }
}
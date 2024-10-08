using Data.Contracts;
using FileSystem.Contracts;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.Notifications;

public record FileMergeFinishedNotification(int FileTaskId) : INotification;

public class FileMergeFinishedHandler : INotificationHandler<FileMergeFinishedNotification>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly IFileMergeSystem _fileMergeSystem;

    public FileMergeFinishedHandler(
        IPlexRipperDbContext dbContext,
        IMediator mediator,
        IFileMergeSystem fileMergeSystem
    )
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _fileMergeSystem = fileMergeSystem;
    }

    public async Task Handle(FileMergeFinishedNotification notification, CancellationToken cancellationToken)
    {
        var fileTask = await _dbContext.FileTasks.GetAsync(notification.FileTaskId, cancellationToken);
        if (fileTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(FileTask), notification.FileTaskId).LogError();
            return;
        }

        var downloadTask = await _dbContext.GetDownloadTaskAsync(
            fileTask.DownloadTaskId,
            cancellationToken: cancellationToken
        );
        if (downloadTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), fileTask.DownloadTaskId).LogError();
            return;
        }

        _fileMergeSystem.DeleteDirectoryFromFilePath(fileTask.FilePaths.First());

        await _dbContext.SetDownloadStatus(downloadTask.ToKey(), DownloadStatus.Completed);

        await _dbContext.FileTasks.Where(x => x.Id == notification.FileTaskId).ExecuteDeleteAsync(cancellationToken);

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTask.ToKey()), cancellationToken);
    }
}

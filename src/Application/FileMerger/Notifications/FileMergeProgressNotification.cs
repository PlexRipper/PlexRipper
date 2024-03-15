using Data.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public record FileMergeProgressNotification(FileMergeProgress Progress) : INotification;

public class FileMergeProgressHandler : INotificationHandler<FileMergeProgressNotification>
{
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public FileMergeProgressHandler(IMediator mediator, IPlexRipperDbContext dbContext, ISignalRService signalRService)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Handle(FileMergeProgressNotification notification, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(notification.Progress.ToKey(), cancellationToken);
        if (downloadTask is null)
        {
            ResultExtensions.EntityNotFound(nameof(DownloadTaskGeneric), notification.Progress.DownloadTaskId).LogError();
            return;
        }

        downloadTask.Percentage = notification.Progress.Percentage;
        downloadTask.FileTransferSpeed = notification.Progress.TransferSpeed;
        downloadTask.DataTotal = notification.Progress.DataTotal;

        await _dbContext.UpdateDownloadProgress(downloadTask.ToKey(), downloadTask, cancellationToken);

        await _signalRService.SendFileMergeProgressUpdateAsync(notification.Progress, cancellationToken);

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTask.ToKey()), cancellationToken);
    }
}
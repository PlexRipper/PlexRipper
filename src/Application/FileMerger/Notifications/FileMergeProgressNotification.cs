using Application.Contracts;
using Data.Contracts;
using Logging.Interface;

namespace PlexRipper.Application.Notifications;

public record FileMergeProgressNotification(FileMergeProgress Progress) : INotification;

public class FileMergeProgressHandler : INotificationHandler<FileMergeProgressNotification>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ISignalRService _signalRService;

    public FileMergeProgressHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ISignalRService signalRService
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _signalRService = signalRService;
    }

    public async Task Handle(FileMergeProgressNotification notification, CancellationToken cancellationToken)
    {
        var downloadTask = await _dbContext.GetDownloadTaskAsync(notification.Progress.ToKey(), cancellationToken);
        if (downloadTask is null)
        {
            ResultExtensions
                .EntityNotFound(nameof(DownloadTaskGeneric), notification.Progress.DownloadTaskId)
                .LogError();
            return;
        }

        _log.DebugLine(notification.Progress.ToString());

        downloadTask.Percentage = notification.Progress.Percentage;
        downloadTask.FileTransferSpeed = notification.Progress.TransferSpeed;
        downloadTask.DataTotal = notification.Progress.DataTotal;

        await _dbContext.UpdateDownloadProgress(downloadTask.ToKey(), downloadTask, cancellationToken);

        await _signalRService.SendFileMergeProgressUpdateAsync(notification.Progress, cancellationToken);

        await _mediator.Send(new DownloadTaskUpdatedNotification(downloadTask.ToKey()), cancellationToken);
    }
}

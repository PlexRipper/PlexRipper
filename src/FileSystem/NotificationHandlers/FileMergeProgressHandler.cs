using Data.Contracts;
using DownloadManager.Contracts;
using Microsoft.EntityFrameworkCore;
using WebAPI.Contracts;

namespace PlexRipper.FileSystem;

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
        await _dbContext.DownloadTasks.Where(x => x.Id == notification.Progress.DownloadTaskId)
            .ExecuteUpdateAsync(p => p
                .SetProperty(x => x.Percentage, notification.Progress.Percentage)
                .SetProperty(x => x.FileTransferSpeed, notification.Progress.TransferSpeed)
                .SetProperty(x => x.DataTotal, notification.Progress.DataTotal), cancellationToken);

        await _signalRService.SendFileMergeProgressUpdateAsync(notification.Progress, cancellationToken);

        await _mediator.Send(new DownloadTaskUpdated(notification.Progress.DownloadTaskId), cancellationToken);
    }
}
using Data.Contracts;
using DownloadManager.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.FileSystem;

public class FileMergeProgressHandler : INotificationHandler<FileMergeProgressNotification>
{
    private readonly IMediator _mediator;
    private readonly ISignalRService _signalRService;

    public FileMergeProgressHandler(IMediator mediator, ISignalRService signalRService)
    {
        _mediator = mediator;
        _signalRService = signalRService;
    }

    public async Task Handle(FileMergeProgressNotification notification, CancellationToken cancellationToken)
    {
        var downloadTaskResult = await _mediator.Send(new UpdateDownloadTaskWithFileMergeProgressByIdCommand(notification.Progress), cancellationToken);
        if (downloadTaskResult.IsFailed)
        {
            downloadTaskResult.LogError();
            return;
        }

        await _signalRService.SendFileMergeProgressUpdateAsync(notification.Progress, cancellationToken);

        await _mediator.Send(new DownloadTaskUpdated(notification.Progress.DownloadTaskId), cancellationToken);
    }
}
using WebAPI.Contracts;

namespace PlexRipper.FileSystem;

public class FileMergeProgressHandler : INotificationHandler<FileMergeProgressNotification>
{
    private readonly ISignalRService _signalRService;

    public FileMergeProgressHandler(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task Handle(FileMergeProgressNotification notification, CancellationToken cancellationToken)
    {
        await _signalRService.SendFileMergeProgressUpdate(notification.Progress);
    }
}
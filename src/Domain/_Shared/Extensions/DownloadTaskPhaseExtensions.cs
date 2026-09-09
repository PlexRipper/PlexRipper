namespace Reaparr.Domain;

public static class DownloadTaskPhaseExtensions
{
    private static readonly ILogger _log = LogFactory.Create(typeof(DownloadTaskPhaseExtensions));

    public static DownloadTaskPhase ToDownloadTaskPhase(this DownloadStatus downloadStatus)
    {
        switch (downloadStatus)
        {
            case DownloadStatus.Queued:
                return DownloadTaskPhase.None;

            case DownloadStatus.Downloading:
            case DownloadStatus.Error:
            case DownloadStatus.Paused:
            case DownloadStatus.AutoPaused:
            case DownloadStatus.Stopped:
            case DownloadStatus.Deleted:
            case DownloadStatus.ServerUnreachable:
            case DownloadStatus.AuthError:
            case DownloadStatus.StorageError:
            case DownloadStatus.SourceUnavailable:
            case DownloadStatus.DownloadClientError:
            case DownloadStatus.IntegrityError:
            case DownloadStatus.Restarting:
                return DownloadTaskPhase.Downloading;

            case DownloadStatus.DownloadFinished:
            case DownloadStatus.Moving:
            case DownloadStatus.MovePaused:
            case DownloadStatus.AutoMovePaused:
            case DownloadStatus.MoveFinished:
            case DownloadStatus.MoveError:
                return DownloadTaskPhase.FileTransfer;

            case DownloadStatus.Completed:
                return DownloadTaskPhase.Completed;

            case DownloadStatus.Unknown:
            default:
                _log.Here().Error("Unknown download task phase with downloadStatus {DownloadStatus}", downloadStatus);
                return DownloadTaskPhase.Unknown;
        }
    }

    public static decimal Percentage(
        DownloadTaskPhase phase,
        IDownloadTaskProgress downloadTaskProgress,
        IDownloadFileTransferProgress fileTransferProgress
    ) =>
        phase switch
        {
            DownloadTaskPhase.FileTransfer => DataFormat.GetPercentage(
                fileTransferProgress.FileDataTransferred,
                downloadTaskProgress.DataTotal
            ),
            DownloadTaskPhase.Completed => 100m,
            _ => DataFormat.GetPercentage(downloadTaskProgress.DataReceived, downloadTaskProgress.DataTotal),
        };

    public static int TimeRemaining(
        DownloadTaskPhase phase,
        IDownloadTaskProgress downloadTaskProgress,
        IDownloadFileTransferProgress fileTransferProgress
    ) =>
        phase switch
        {
            DownloadTaskPhase.FileTransfer => DataFormat.GetTimeRemaining(
                downloadTaskProgress.DataTotal - fileTransferProgress.FileDataTransferred,
                fileTransferProgress.FileTransferSpeed
            ),
            DownloadTaskPhase.Completed => 0,
            _ => DataFormat.GetTimeRemaining(
                downloadTaskProgress.DataTotal - downloadTaskProgress.DataReceived,
                downloadTaskProgress.DownloadSpeed
            ),
        };

    public static long Speed(
        DownloadTaskPhase phase,
        IDownloadTaskProgress downloadTaskProgress,
        IDownloadFileTransferProgress fileTransferProgress
    ) =>
        phase switch
        {
            DownloadTaskPhase.FileTransfer => fileTransferProgress.FileTransferSpeed,
            DownloadTaskPhase.Completed => 0,
            _ => downloadTaskProgress.DownloadSpeed,
        };
}

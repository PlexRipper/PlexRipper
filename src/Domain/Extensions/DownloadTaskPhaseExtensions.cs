using Reaparr.Logging;

namespace Reaparr.Domain;

public static class DownloadTaskPhaseExtensions
{
    private static readonly ILog _log = new LogConfig().CreateLogInstance(typeof(DownloadTaskPhaseExtensions));

    public static DownloadTaskPhase ToDownloadTaskPhase(this DownloadStatus downloadStatus)
    {
        switch (downloadStatus)
        {
            case DownloadStatus.Queued:
                return DownloadTaskPhase.None;

            case DownloadStatus.Downloading:
            case DownloadStatus.DownloadFinished:
            case DownloadStatus.Error:
            case DownloadStatus.Paused:
            case DownloadStatus.Stopped:
            case DownloadStatus.Deleted:
            case DownloadStatus.ServerUnreachable:
                return DownloadTaskPhase.Downloading;

            case DownloadStatus.Merging:
            case DownloadStatus.Moving:
            case DownloadStatus.MergePaused:
            case DownloadStatus.MovePaused:
            case DownloadStatus.MergeFinished:
            case DownloadStatus.MoveFinished:
            case DownloadStatus.MoveError:
            case DownloadStatus.MergeError:
                return DownloadTaskPhase.FileTransfer;

            case DownloadStatus.Completed:
                return DownloadTaskPhase.Completed;

            case DownloadStatus.Unknown:
            default:
                _log.Error("Unknown download task phase with downloadStatus {DownloadStatus}.", downloadStatus);
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
            _ => DataFormat.GetPercentage(downloadTaskProgress.DataReceived, downloadTaskProgress.DataTotal),
        };

    public static long TimeRemaining(
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

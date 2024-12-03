using Logging.Interface;

namespace PlexRipper.Domain;

public static class DownloadTaskPhaseExtensions
{
    private static ILog _log = LogManager.CreateLogInstance(typeof(DownloadTaskPhaseExtensions));

    public static DownloadTaskPhase FromPercentage(
        IDownloadTaskProgress downloadTaskProgress,
        IDownloadFileTransferProgress fileTransferProgress
    )
    {
        var downloadPercentage = DataFormat.GetPercentage(
            downloadTaskProgress.DataReceived,
            downloadTaskProgress.DataTotal
        );
        var fileTransferPercentage = DataFormat.GetPercentage(
            fileTransferProgress.FileDataTransferred,
            downloadTaskProgress.DataTotal
        );

        if (downloadPercentage == 0 && fileTransferPercentage == 0)
            return DownloadTaskPhase.None;

        if (downloadPercentage is > 0 and < 100 && fileTransferPercentage == 0)
            return DownloadTaskPhase.Downloading;

        if (downloadPercentage == 100 && fileTransferPercentage is >= 0 and < 100)
            return DownloadTaskPhase.FileTransfer;

        if (downloadPercentage == 100 && fileTransferPercentage == 100)
            return DownloadTaskPhase.Completed;

        _log.Error(
            "Unknown download task phase with downloadPercentage {DownloadPercentage} and fileTransferPercentage {FileTransferPercentage}.",
            downloadPercentage,
            fileTransferPercentage
        );

        return DownloadTaskPhase.Unknown;
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

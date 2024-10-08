namespace PlexRipper.BaseTests;

public static partial class DownloadTaskExtensions
{
    public static DownloadTaskGeneric SetDownloadStatus(
        this DownloadTaskGeneric downloadTask,
        DownloadStatus downloadStatus
    )
    {
        downloadTask.DownloadStatus = downloadStatus;
        if (downloadTask.Children.Any())
            downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);

        return downloadTask;
    }

    public static List<DownloadTaskGeneric> SetDownloadStatus(
        this List<DownloadTaskGeneric> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.DownloadStatus = downloadStatus;
            if (downloadTask.Children.Any())
                downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);
        }

        return downloadTasks;
    }
}

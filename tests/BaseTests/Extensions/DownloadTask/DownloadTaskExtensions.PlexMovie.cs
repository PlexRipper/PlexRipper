using Data.Contracts;

namespace PlexRipper.BaseTests;

public static partial class DownloadTaskExtensions
{
    public static DownloadTaskMovie SetDownloadStatus(
        this DownloadTaskMovie downloadTask,
        DownloadStatus downloadStatus
    )
    {
        downloadTask.DownloadStatus = downloadStatus;
        if (downloadTask.Children.Any())
            downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);

        return downloadTask;
    }

    public static List<DownloadTaskMovie> SetDownloadStatus(
        this List<DownloadTaskMovie> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.DownloadStatus = downloadStatus;
            if (downloadTask.Children.Any())
                downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);

            downloadTask.Calculate();
        }

        return downloadTasks;
    }

    public static List<DownloadTaskMovieFile> SetDownloadStatus(
        this List<DownloadTaskMovieFile> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.DownloadStatus = downloadStatus;
            downloadTask.SetPercentageBasedOnStatus(downloadStatus);
        }

        return downloadTasks;
    }
}

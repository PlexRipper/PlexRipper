using Reaparr.Data.Contracts;

namespace Reaparr.BaseTests;

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

    public static ICollection<DownloadTaskMovie> SetDownloadStatus(
        this ICollection<DownloadTaskMovie> downloadTasks,
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

    public static ICollection<DownloadTaskMovieFile> SetDownloadStatus(
        this ICollection<DownloadTaskMovieFile> downloadTasks,
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

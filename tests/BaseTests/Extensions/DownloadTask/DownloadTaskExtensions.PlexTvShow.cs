using Reaparr.Data.Contracts;

namespace Reaparr.BaseTests;

public static partial class DownloadTaskExtensions
{
    public static ICollection<DownloadTaskTvShow> SetDownloadStatus(
        this ICollection<DownloadTaskTvShow> downloadTasks,
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

    public static DownloadTaskTvShow SetDownloadStatus(
        this DownloadTaskTvShow downloadTask,
        DownloadStatus downloadStatus
    )
    {
        downloadTask.DownloadStatus = downloadStatus;
        if (downloadTask.Children.Any())
            downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);

        downloadTask.Calculate();

        return downloadTask;
    }

    public static DownloadTaskTvShowSeason SetDownloadStatus(
        this DownloadTaskTvShowSeason downloadTask,
        DownloadStatus downloadStatus
    )
    {
        downloadTask.DownloadStatus = downloadStatus;
        if (downloadTask.Children.Any())
            downloadTask.Children = downloadTask.Children.SetDownloadStatus(downloadStatus);

        downloadTask.Calculate();

        return downloadTask;
    }

    public static ICollection<DownloadTaskTvShowSeason> SetDownloadStatus(
        this ICollection<DownloadTaskTvShowSeason> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
            downloadTask.SetDownloadStatus(downloadStatus);

        return downloadTasks;
    }

    public static ICollection<DownloadTaskTvShowEpisode> SetDownloadStatus(
        this ICollection<DownloadTaskTvShowEpisode> downloadTasks,
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

    public static ICollection<DownloadTaskTvShowEpisodeFile> SetDownloadStatus(
        this ICollection<DownloadTaskTvShowEpisodeFile> downloadTasks,
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

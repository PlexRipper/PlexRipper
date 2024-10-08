namespace PlexRipper.BaseTests;

public static partial class DownloadTaskExtensions
{
    public static List<DownloadTaskTvShow> SetDownloadStatus(
        this List<DownloadTaskTvShow> downloadTasks,
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

        return downloadTask;
    }

    public static List<DownloadTaskTvShowSeason> SetDownloadStatus(
        this List<DownloadTaskTvShowSeason> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
            downloadTask.SetDownloadStatus(downloadStatus);

        return downloadTasks;
    }

    public static List<DownloadTaskTvShowEpisode> SetDownloadStatus(
        this List<DownloadTaskTvShowEpisode> downloadTasks,
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

    public static List<DownloadTaskTvShowEpisodeFile> SetDownloadStatus(
        this List<DownloadTaskTvShowEpisodeFile> downloadTasks,
        DownloadStatus downloadStatus
    )
    {
        foreach (var downloadTask in downloadTasks)
            downloadTask.DownloadStatus = downloadStatus;

        return downloadTasks;
    }
}

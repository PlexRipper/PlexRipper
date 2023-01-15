namespace PlexRipper.Domain;

public static class ListDownloadTaskExtensions
{
    public static List<DownloadTask> SetIds(this List<DownloadTask> downloadTasks, int plexServerId, int plexLibraryId, string serverMachineId)
    {
        List<DownloadTask> SetIdsOnDownloadTasks(List<DownloadTask> childDownloadTasks)
        {
            if (childDownloadTasks is null)
                return new List<DownloadTask>();

            foreach (var downloadTask in childDownloadTasks)
            {
                downloadTask.PlexLibraryId = plexLibraryId;
                downloadTask.PlexServerId = plexServerId;
                downloadTask.ServerMachineIdentifier = serverMachineId;
                if (downloadTask.Children is not null && downloadTask.Children.Any())
                    SetIdsOnDownloadTasks(downloadTask.Children);
            }

            return childDownloadTasks;
        }

        return SetIdsOnDownloadTasks(downloadTasks);
    }

    public static List<DownloadTask> SetToCompleted(this List<DownloadTask> downloadTasks)
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.DownloadStatus = DownloadStatus.Completed;
            if (downloadTask.Children is not null && downloadTask.Children.Any())
                downloadTask.Children = SetToCompleted(downloadTask.Children);
        }

        return downloadTasks;
    }

    public static List<DownloadTask> SetToDownloading(this List<DownloadTask> downloadTasks)
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.DownloadStatus = DownloadStatus.Completed;
            if (downloadTask.Children is not null && downloadTask.Children.Any())
                downloadTask.Children = SetToDownloading(downloadTask.Children);
        }

        return downloadTasks;
    }

    public static List<DownloadTask> SetDownloadUrl(this List<DownloadTask> downloadTasks, PlexServerConnection plexServerConnection, string fileUrl)
    {
        foreach (var downloadTask in downloadTasks)
        {
            if (downloadTask.DownloadTaskType
                is DownloadTaskType.EpisodeData
                or DownloadTaskType.EpisodePart
                or DownloadTaskType.MovieData
                or DownloadTaskType.MoviePart)
            {
                downloadTask.DownloadUrl = new UriBuilder()
                {
                    Scheme = plexServerConnection.Protocol,
                    Host = plexServerConnection.Address,
                    Port = plexServerConnection.Port,
                    Path = fileUrl,
                    Query = "X-Plex-Token=SomeRandomToken",
                }.ToString();
            }

            if (downloadTask.Children is not null && downloadTask.Children.Any())
                downloadTask.Children = SetDownloadUrl(downloadTask.Children, plexServerConnection, fileUrl);
        }

        return downloadTasks;
    }

    public static List<DownloadTask> SetRootId(this List<DownloadTask> downloadTasks, int rootTaskId)
    {
        foreach (var downloadTask in downloadTasks)
        {
            downloadTask.RootDownloadTaskId = rootTaskId;
            if (downloadTask.Children is null || !downloadTask.Children.Any())
                continue;

            downloadTask.Children = SetRootId(downloadTask.Children, rootTaskId);
        }

        return downloadTasks;
    }
}
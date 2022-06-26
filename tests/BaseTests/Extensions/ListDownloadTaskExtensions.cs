using PlexRipper.Domain;

namespace PlexRipper.BaseTests.Extensions
{
    public static class ListDownloadTaskExtensions
    {
        public static List<DownloadTask> SetIds(this List<DownloadTask> downloadTasks, int plexServerId, int plexLibraryId)
        {
            List<DownloadTask> SetIdsOnDownloadTasks(List<DownloadTask> childDownloadTasks)
            {
                if (childDownloadTasks is null)
                    return new List<DownloadTask>();

                foreach (var downloadTask in childDownloadTasks)
                {
                    downloadTask.PlexLibraryId = plexLibraryId;
                    downloadTask.PlexServerId = plexServerId;
                    if (downloadTask.Children is not null && downloadTask.Children.Any())
                    {
                        SetIdsOnDownloadTasks(downloadTask.Children);
                    }
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
                {
                    downloadTask.Children = SetToCompleted(downloadTask.Children);
                }
            }

            return downloadTasks;
        }

        public static List<DownloadTask> SetToDownloading(this List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                downloadTask.DownloadStatus = DownloadStatus.Completed;
                if (downloadTask.Children is not null && downloadTask.Children.Any())
                {
                    downloadTask.Children = SetToDownloading(downloadTask.Children);
                }
            }

            return downloadTasks;
        }
    }
}
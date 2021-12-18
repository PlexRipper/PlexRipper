using System.Collections.Generic;
using System.Linq;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests.Extensions
{
    public static class ListDownloadTaskExtensions
    {
        public static List<DownloadTask> SetIds(this List<DownloadTask> downloadTasks, int plexServerId, int plexLibraryId)
        {
            List<DownloadTask> SetIdsOnDownloadTasks(List<DownloadTask> childDownloadTasks)
            {
                foreach (var downloadTask in childDownloadTasks)
                {
                    downloadTask.PlexLibraryId = plexLibraryId;
                    downloadTask.PlexServerId = plexServerId;
                    if (downloadTask.Children.Any())
                    {
                        SetIdsOnDownloadTasks(downloadTask.Children);
                    }
                }

                return childDownloadTasks;
            }

            return SetIdsOnDownloadTasks(downloadTasks);
        }
    }
}
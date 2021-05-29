using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>.
    /// </summary>
    public class DownloadQueue : IDownloadQueue
    {
        public Subject<DownloadClientUpdate> UpdateDownloadClient { get; } = new();

        public Subject<int> StartDownloadTask { get; } = new();

        public void ExecuteDownloadQueue(List<PlexServer> plexServers)
        {
            if (!plexServers.Any())
            {
                Log.Information("There are no PlexServers with DownloadTasks");
                return;
            }

            Log.Information($"Starting the check of {plexServers.Count} PlexServers.");
            foreach (var plexServer in plexServers)
            {
                var downloadTasks = plexServer.PlexLibraries.SelectMany(x => x.DownloadTasks).ToList();

                // Set all initialized to Queued
                foreach (var downloadTask in downloadTasks)
                {
                    if (downloadTask.DownloadStatus == DownloadStatus.Initialized)
                    {
                        downloadTask.DownloadStatus = DownloadStatus.Queued;
                        UpdateDownloadClient.OnNext(new DownloadClientUpdate(downloadTask));
                    }
                }

                // Retrieve next download task queued
                var downloadingTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Downloading);
                if (downloadingTask != null)
                {
                    Log.Warning($"PlexServer: {plexServer.Name} already has a download which is in currently downloading");
                    continue;
                }

                var queuedDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
                if (queuedDownloadTask != null)
                {
                    Log.Debug(
                        $"Starting the next Queued downloadTask with id {queuedDownloadTask.Id} - {queuedDownloadTask.Title} for server {plexServer.Name}");
                    queuedDownloadTask.DownloadStatus = DownloadStatus.Downloading;
                    UpdateDownloadClient.OnNext(new DownloadClientUpdate(queuedDownloadTask));
                    StartDownloadTask.OnNext(queuedDownloadTask.Id);
                    return;
                }

                Log.Information($"There are no available downloadTasks remaining for PlexServer: {plexServer.Name}");
            }
        }
    }
}
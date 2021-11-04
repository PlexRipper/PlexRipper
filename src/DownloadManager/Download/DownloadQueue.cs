using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentResults;
using Logging;
using MediatR;
using PlexRipper.Application;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    /// <summary>
    /// The DownloadQueue is responsible for deciding which downloadTask is handled by the <see cref="DownloadManager"/>.
    /// </summary>
    public class DownloadQueue : IDownloadQueue
    {
        private readonly IMediator _mediator;

        public DownloadQueue(IMediator mediator)
        {
            _mediator = mediator;
        }
        #region Properties

        public Subject<DownloadTask> StartDownloadTask { get; } = new();

        public Subject<List<DownloadTask>> UpdateDownloadTasks { get; } = new();

        #endregion

        #region Public Methods

        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        public async Task CheckDownloadQueue()
        {
            Log.Debug("Checking for download tasks which can be processed.");
            var serverListResult = await _mediator.Send(new GetAllDownloadTasksInPlexServersQuery(true));
            var serverList = serverListResult.Value.Where(x => x.HasDownloadTasks).ToList();

            ExecuteDownloadQueue(serverList);
        }

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
                downloadTasks = SetToQueued(downloadTasks);

                var nextDownloadTask = GetNextDownloadTask(ref downloadTasks);
                if (nextDownloadTask.IsFailed)
                {
                    Log.Information($"There are no available downloadTasks remaining for PlexServer: {plexServer.Name}");
                    continue;
                }

                Log.Information($"Selected download task {nextDownloadTask.Value.FullTitle} to start as the next task");
                downloadTasks = SetToDownloading(downloadTasks);
                UpdateDownloadTasks.OnNext(downloadTasks);

                StartDownloadTask.OnNext(nextDownloadTask.Value);
            }
        }

        #endregion

        #region Private Methods

        private Result<DownloadTask> GetNextDownloadTask(ref List<DownloadTask> downloadTasks)
        {
            // Check if there is anything downloading already
            var nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Downloading);
            if (nextDownloadTask is null)
            {
                nextDownloadTask = downloadTasks.FirstOrDefault(x => x.DownloadStatus == DownloadStatus.Queued);
                if (nextDownloadTask is null)
                    return Result.Fail("There were no downloadTasks left to download").LogDebug();
            }

            if (!nextDownloadTask.Children.Any())
            {
                nextDownloadTask.DownloadStatus = DownloadStatus.Downloading;
                return Result.Ok(nextDownloadTask);
            }

            var children = nextDownloadTask.Children;
            return GetNextDownloadTask(ref children);
        }

        private List<DownloadTask> SetToDownloading(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.Children.Any())
                {
                    downloadTask.Children = SetToDownloading(downloadTask.Children);
                }

                if (downloadTask.Children.Any(x => x.DownloadStatus is DownloadStatus.Downloading))
                {
                    downloadTask.DownloadStatus = DownloadStatus.Downloading;
                }
            }

            return downloadTasks;
        }

        private List<DownloadTask> SetToQueued(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.DownloadStatus is DownloadStatus.Initialized)
                {
                    downloadTask.DownloadStatus = DownloadStatus.Queued;
                }

                if (downloadTask.Children.Any())
                {
                    downloadTask.Children = SetToQueued(downloadTask.Children);
                }
            }

            return downloadTasks;
        }

        #endregion
    }
}
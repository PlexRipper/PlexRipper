using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
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

        private readonly Subject<DownloadTask> _startDownloadTask = new();

        private readonly Subject<List<DownloadTask>> _updateDownloadTasks = new();

        private readonly Subject<int> _serverCompletedDownloading = new();

        #endregion

        public IObservable<DownloadTask> StartDownloadTask => _startDownloadTask.AsObservable();

        public IObservable<List<DownloadTask>> UpdateDownloadTasks => _updateDownloadTasks.AsObservable();

        /// <summary>
        /// Emits the id of a <see cref="PlexServer"/> which has no more <see cref="DownloadTask">DownloadTasks</see> to process.
        /// </summary>
        public IObservable<int> ServerCompletedDownloading => _serverCompletedDownloading.AsObservable();

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

        // TODO Might need to do this on a per-server level otherwise many server might influence each other
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
                downloadTasks = SetToCompleted(downloadTasks);

                var nextDownloadTask = GetNextDownloadTask(ref downloadTasks);
                if (nextDownloadTask.IsFailed)
                {
                    Log.Information($"There are no available downloadTasks remaining for PlexServer: {plexServer.Name}");
                    _serverCompletedDownloading.OnNext(plexServer.Id);
                    continue;
                }

                Log.Information($"Selected download task {nextDownloadTask.Value.FullTitle} to start as the next task");
                downloadTasks = SetToDownloading(downloadTasks);
                _updateDownloadTasks.OnNext(downloadTasks);

                _startDownloadTask.OnNext(nextDownloadTask.Value);
            }
        }

        public Result<DownloadTask> GetNextDownloadTask(ref List<DownloadTask> downloadTasks)
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

        public List<DownloadTask> SetToDownloading(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.Children.Any())
                {
                    downloadTask.Children = SetToDownloading(downloadTask.Children);
                }

                // Only set the parent(s) of the downloadable Tasks to download
                // because the DownloadClient decides the downloading status of downloadable tasks
                if (!downloadTask.IsDownloadable && downloadTask.Children.Any(x => x.DownloadStatus is DownloadStatus.Downloading))
                {
                    downloadTask.DownloadStatus = DownloadStatus.Downloading;
                }
            }

            return downloadTasks;
        }

        public List<DownloadTask> SetToQueued(List<DownloadTask> downloadTasks)
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

        public List<DownloadTask> SetToCompleted(List<DownloadTask> downloadTasks)
        {
            foreach (var downloadTask in downloadTasks)
            {
                if (downloadTask.Children.Any())
                {
                    downloadTask.Children = SetToCompleted(downloadTask.Children);
                    if (downloadTask.Children.All(x => x.DownloadStatus is DownloadStatus.Completed))
                    {
                        downloadTask.DownloadStatus = DownloadStatus.Completed;
                    }
                }
            }

            return downloadTasks;
        }

        #endregion
    }
}
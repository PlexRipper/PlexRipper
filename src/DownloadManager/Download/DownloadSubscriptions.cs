using System.Collections.Generic;
using FluentResults;
using PlexRipper.Application.Common;
using PlexRipper.Domain;
using PlexRipper.Domain.RxNet;

namespace PlexRipper.DownloadManager
{
    public class DownloadSubscriptions : IDownloadSubscriptions
    {
        private readonly IDownloadTracker _downloadTracker;

        private readonly IDownloadQueue _downloadQueue;

        private readonly IDownloadCommands _downloadCommands;

        private readonly IDownloadProgressScheduler _downloadProgressScheduler;

        public DownloadSubscriptions(
            IDownloadTracker downloadTracker,
            IDownloadQueue downloadQueue,
            IDownloadCommands downloadCommands,
            IDownloadProgressScheduler downloadProgressScheduler)
        {
            _downloadTracker = downloadTracker;
            _downloadQueue = downloadQueue;
            _downloadCommands = downloadCommands;
            _downloadProgressScheduler = downloadProgressScheduler;
        }

        public Result Setup()
        {
            _downloadTracker
                .DownloadTaskStart
                .SubscribeAsync(async downloadTask =>
                    await _downloadProgressScheduler.StartDownloadProgressJob(downloadTask.PlexServerId));

            // Setup DownloadQueue subscriptions
            _downloadQueue
                .StartDownloadTask
                .SubscribeAsync(async downloadTask => await _downloadCommands.StartDownloadTaskAsync(downloadTask));

            // Halt progress updates for PlexServers that are no more active
            _downloadQueue
                .ServerCompletedDownloading
                .SubscribeAsync(async plexServerId => await _downloadProgressScheduler.StopDownloadProgressJob(plexServerId));

            return Result.Ok();
        }
    }
}
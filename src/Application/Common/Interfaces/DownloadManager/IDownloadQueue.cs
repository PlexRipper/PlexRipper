using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IDownloadQueue : ISetup
    {
        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        Task CheckDownloadQueue(List<int> plexServerIds = null);

        IObservable<DownloadTask> StartDownloadTask { get; }

        IObservable<List<DownloadTask>> UpdateDownloadTasks { get; }

        IObservable<int> ServerCompletedDownloading { get; }

        Task<Result> CheckDownloadQueueServer(int plexServerId);
    }
}
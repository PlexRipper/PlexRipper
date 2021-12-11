using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IDownloadQueue
    {
        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        Task CheckDownloadQueue();

        IObservable<DownloadTask> StartDownloadTask { get; }

        IObservable<List<DownloadTask>> UpdateDownloadTasks { get; }

        IObservable<int> ServerCompletedDownloading { get; }
    }
}
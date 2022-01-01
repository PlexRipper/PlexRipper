using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface IDownloadQueue : ISetup, IBusy
    {
        /// <summary>
        /// Check the DownloadQueue for downloadTasks which can be started.
        /// </summary>
        Task<Result> CheckDownloadQueue(List<int> plexServerIds);

        IObservable<DownloadTask> StartDownloadTask { get; }

        IObservable<int> ServerCompletedDownloading { get; }

        Task<Result> CheckDownloadQueueServer(int plexServerId);

        /// <summary>
        /// Adds a list of <see cref="DownloadTask"/>s to the download queue.
        /// </summary>
        /// <param name="downloadTasks">The list of <see cref="DownloadTask"/>s that will be checked and added.</param>
        /// <returns>Returns true if all downloadTasks were added successfully.</returns>
        Task<Result> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks);
    }
}
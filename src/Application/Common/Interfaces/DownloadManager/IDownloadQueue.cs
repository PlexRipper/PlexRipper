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

        /// <summary>
        ///  Determines the next downloadable <see cref="DownloadTask"/>.
        /// Will only return a successful result if a queued task can be found
        /// </summary>
        /// <param name="downloadTasks"></param>
        /// <returns></returns>
        Result<DownloadTask> GetNextDownloadTask(List<DownloadTask> downloadTasks);
    }
}
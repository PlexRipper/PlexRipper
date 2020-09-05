using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain.Entities;

namespace PlexRipper.Application.Common
{
    public interface IDownloadManager
    {
        Result<bool> StopDownload(int downloadTaskId);

        /// <summary>
        /// Adds a single DownloadTask to the Download queue
        /// </summary>
        /// <param name="downloadTask">The <see cref="DownloadTask"/> that will be checked and added.</param>
        /// <param name="performCheck">Should the CheckDownloadQueue() be called at the end</param>
        /// <returns>Returns true if successfully added and false if the downloadTask already exists</returns>
        Task<Result<bool>> AddToDownloadQueueAsync(DownloadTask downloadTask, bool performCheck = true);

        /// <summary>
        /// Adds a list of <see cref="DownloadTask"/>s to the download queue.
        /// </summary>
        /// <param name="downloadTasks">The list of <see cref="DownloadTask"/>s that will be checked and added.</param>
        /// <returns>Returns true if all downloadTasks were added successfully.</returns>
        Task<Result<bool>> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks);

        Task<Result<bool>> RestartDownloadAsync(int downloadTaskId);
        Task<Result<bool>> ClearCompletedAsync();

        /// <summary>
        /// Starts a queued task immediately.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to start.</param>
        /// <returns>Is successful.</returns>
        Task<Result<bool>> StartDownload(int downloadTaskId);

        /// <summary>
        /// Pause a currently downloading <see cref="DownloadTask"/>.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/> to pause.</param>
        /// <returns>Is successful.</returns>
        Result<bool> PauseDownload(int downloadTaskId);

        /// <summary>
        /// Deletes the PlexDownloadClient from the _downloadList and executes its disposal.
        /// </summary>
        /// <param name="downloadTaskId">The id of PlexDownloadClient to delete,
        /// the <see cref="DownloadTask"/> id can be used as these are always the same.</param>
        void DeleteDownloadClient(int downloadTaskId);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    /// <summary>
    /// Interface for the DownloadManager.
    /// </summary>
    public interface IDownloadManager
    {


        /// <summary>
        /// Adds a list of <see cref="DownloadTask"/>s to the download queue.
        /// </summary>
        /// <param name="downloadTasks">The list of <see cref="DownloadTask"/>s that will be checked and added.</param>
        /// <returns>Returns true if all downloadTasks were added successfully.</returns>
        Task<Result> AddToDownloadQueueAsync(List<DownloadTask> downloadTasks);




    }
}
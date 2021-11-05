using System;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;
using PlexRipper.DownloadManager.Download;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadTracker
    {
        Task<Result<PlexDownloadClient>> CreateDownloadClient(int downloadTaskId);

        /// <summary>
        /// Check if a <see cref="PlexDownloadClient"/> has already been assigned to this <see cref="DownloadTask"/>.
        /// </summary>
        /// <param name="downloadTaskId">The id of the <see cref="DownloadTask"/>.</param>
        /// <returns>Returns the <see cref="PlexDownloadClient"/> if found and fails otherwise.</returns>
        Result<PlexDownloadClient> GetDownloadClient(int downloadTaskId);

        /// <summary>
        /// Deletes and disposes the <see cref="PlexDownloadClient"/> from the <see cref="DownloadManager"/>
        /// </summary>
        /// <param name="downloadTaskId">The <see cref="PlexDownloadClient"/> with this downloadTaskId</param>
        void DeleteDownloadClient(int downloadTaskId);

        IObservable<DownloadTask> DownloadTaskUpdate { get; }

        int ActiveDownloadClients { get; }
    }
}
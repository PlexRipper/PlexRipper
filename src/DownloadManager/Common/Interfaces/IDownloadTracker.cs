using System;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadTracker
    {
        #region Properties

        int ActiveDownloadClients { get; }

        /// <summary>
        /// Gets the observable which fires once a <see cref="DownloadTask"/> has finished downloading.
        /// </summary>
        IObservable<DownloadTask> DownloadTaskFinished { get; }

        IObservable<DownloadTask> DownloadTaskStart { get; }

        IObservable<DownloadTask> DownloadTaskUpdate { get; }

        IObservable<DownloadTask> DownloadTaskStopped { get; }

        IObservable<DownloadTask> DownloadStatusChanged { get; }

        #endregion

        #region Public Methods

        bool IsDownloading(int downloadTaskId);

        Task<Result> PauseDownloadClient(int downloadTaskId);

        Task<Result> StartDownloadClient(int downloadTaskId);

        Task<Result> StopDownloadClient(int downloadTaskId);

        #endregion
    }
}
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadTracker : IBusy, ISetup
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

        Task DownloadProcessTask { get; }

        #endregion

        #region Public Methods

        bool IsDownloading(int downloadTaskId);

        Task<Result> PauseDownloadClient(int downloadTaskId);

        /// <summary>
        /// Will start the download of a <see cref="DownloadTask"/> by it's id.
        /// Will return once it has started
        /// </summary>
        /// <param name="downloadTaskId"></param>
        /// <returns></returns>
        Task<Result> StartDownloadClient(int downloadTaskId);

        Task<Result> StopDownloadClient(int downloadTaskId);

        #endregion

        Result<Task> GetDownloadProcessTask(int downloadTaskId);
    }
}
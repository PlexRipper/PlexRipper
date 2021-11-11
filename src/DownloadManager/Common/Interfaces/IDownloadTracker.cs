using System;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadTracker
    {
        IObservable<DownloadTask> DownloadTaskUpdate { get; }

        IObservable<DownloadTask> DownloadTaskCompleted { get; }

        int ActiveDownloadClients { get; }

        IObservable<DownloadTask> DownloadTaskStart { get; }

        bool IsDownloading(int downloadTaskId);

        Task<Result> StartDownloadClient(int downloadTaskId);

        Task<Result> StopDownloadClient(int downloadTaskId);

        Task<Result> PauseDownloadClient(int downloadTaskId);
    }
}
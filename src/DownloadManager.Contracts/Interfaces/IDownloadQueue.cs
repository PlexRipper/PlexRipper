using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadQueue : ISetup, IBusy
{
    /// <summary>
    /// Check the DownloadQueue for downloadTasks which can be started.
    /// </summary>
    Task<Result> CheckDownloadQueue(List<int> plexServerIds);

    Task<Result<DownloadTask>> CheckDownloadQueueServer(int plexServerId);

    /// <summary>
    ///  Determines the next downloadable <see cref="DownloadTask"/>.
    /// Will only return a successful result if a queued task can be found
    /// </summary>
    /// <param name="downloadTasks"></param>
    /// <returns></returns>
    Result<DownloadTask> GetNextDownloadTask(List<DownloadTask> downloadTasks);
}
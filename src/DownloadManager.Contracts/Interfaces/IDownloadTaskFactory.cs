using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadTaskFactory
{
    /// <summary>
    /// Regenerates <see cref="DownloadTask">DownloadTasks</see> while maintaining the Id and priority.
    /// Will also remove old <see cref="DownloadWorkerTask">DownloadWorkerTasks</see> assigned to the old downloadTasks from the database.
    /// </summary>
    /// <param name="downloadTaskIds"></param>
    /// <returns>A list of newly generated <see cref="DownloadTask">DownloadTasks</see></returns>
    Task<Result<List<DownloadTask>>> RegenerateDownloadTask(List<int> downloadTaskIds);

    Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTask downloadTask);
}
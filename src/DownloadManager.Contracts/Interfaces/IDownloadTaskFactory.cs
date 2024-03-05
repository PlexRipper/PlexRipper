using FluentResults;
using PlexRipper.Domain;

namespace DownloadManager.Contracts;

public interface IDownloadTaskFactory
{
    Result<List<DownloadWorkerTask>> GenerateDownloadWorkerTasks(DownloadTaskFileBase downloadTask);
}
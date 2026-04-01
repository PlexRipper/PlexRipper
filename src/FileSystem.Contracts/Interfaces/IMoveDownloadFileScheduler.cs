using FluentResults;

namespace Reaparr.FileSystem.Contracts;

public interface IMoveDownloadFileScheduler
{
    Task<Result> StartMoveDownloadFileJob(DownloadTaskKey downloadTaskKey);

    Task<Result> StopMoveDownloadFileJob(DownloadTaskKey downloadTaskKey);

    Task<bool> IsDownloadFileMoving(DownloadTaskKey downloadTaskKey);

    Task<bool> IsAnyMoveDownloadFileJobRunning();
}

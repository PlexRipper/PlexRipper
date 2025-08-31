using FluentResults;
using Reaparr.Domain;

namespace Reaparr.FileSystem.Contracts;

public interface IFileMergeScheduler
{
    Task<Result> StartFileMergeJob(DownloadTaskKey downloadTaskKey);

    Task<Result> StopFileMergeJob(DownloadTaskKey downloadTaskKey);

    Task<bool> IsDownloadTaskMerging(DownloadTaskKey downloadTaskKey);

    Task<bool> IsAnyFileMergeJobRunning();
}

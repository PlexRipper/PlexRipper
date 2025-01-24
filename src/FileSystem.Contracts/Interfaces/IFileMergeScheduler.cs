using FluentResults;
using PlexRipper.Domain;

namespace FileSystem.Contracts;

public interface IFileMergeScheduler
{
    Task<Result> StartFileMergeJob(DownloadTaskKey downloadTaskKey);

    Task<Result> StopFileMergeJob(DownloadTaskKey downloadTaskKey);

    Task<bool> IsDownloadTaskMerging(DownloadTaskKey downloadTaskKey);

    Task<bool> IsAnyFileMergeJobRunning();
}

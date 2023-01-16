using FluentResults;
using PlexRipper.Domain;

namespace FileSystem.Contracts;

public interface IFileMergeScheduler
{
    Task<Result> StartFileMergeJob(int fileTaskId);
    Task<Result> StopFileMergeJob(int fileTaskId);

    /// <summary>
    /// Creates an FileTask from a completed <see cref="DownloadTask"/> and adds this to the database.
    /// </summary>
    /// <param name="downloadTaskId"></param>
    Task<Result<DownloadFileTask>> CreateFileTaskFromDownloadTask(int downloadTaskId);
}
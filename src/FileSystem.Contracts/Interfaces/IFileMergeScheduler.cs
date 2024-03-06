using FluentResults;
using PlexRipper.Domain;

namespace FileSystem.Contracts;

public interface IFileMergeScheduler
{
    Task<Result> StartFileMergeJob(int fileTaskId);

    Task<Result> StopFileMergeJob(int fileTaskId);

    /// <summary>
    /// Creates an FileTask from a completed <see cref="DownloadTaskGeneric"/> and adds this to the database.
    /// </summary>
    /// <param name="downloadTaskKey"></param>
    Task<Result<FileTask>> CreateFileTaskFromDownloadTask(DownloadTaskKey downloadTaskKey);
}
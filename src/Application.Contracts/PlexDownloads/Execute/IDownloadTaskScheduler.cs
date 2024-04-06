using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IDownloadTaskScheduler
{
    Task<Result> StartDownloadTaskJob(DownloadTaskKey downloadTaskKey);

    Task<Result> StopDownloadTaskJob(DownloadTaskKey downloadTaskKey);

    Task<bool> IsDownloading(DownloadTaskKey downloadTaskKey);

    Task<bool> IsServerDownloading(int plexServerId);
    Task AwaitDownloadTaskJob(Guid downloadTaskId, CancellationToken cancellationToken = default);
}
using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IDownloadTaskScheduler
{
    Task<Result> StartDownloadTaskJob(DownloadTaskKey downloadTaskKey);

    Task<Result> StopDownloadTaskJob(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default);

    Task<bool> IsDownloading(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken = default);

    Task<bool> IsServerDownloading(int plexServerId);
    Task AwaitDownloadTaskJob(Guid downloadTaskId, CancellationToken cancellationToken = default);
}

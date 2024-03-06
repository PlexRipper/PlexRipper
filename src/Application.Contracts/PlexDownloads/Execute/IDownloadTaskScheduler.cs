using FluentResults;

namespace Application.Contracts;

public interface IDownloadTaskScheduler
{
    Task<Result> StartDownloadTaskJob(Guid downloadTaskId, int plexServerId);
    Task<Result> StopDownloadTaskJob(Guid downloadTaskId);
    Task<bool> IsDownloading(Guid downloadTaskId);

    Task<bool> IsServerDownloading(int plexServerId);
    Task AwaitDownloadTaskJob(Guid downloadTaskId, CancellationToken cancellationToken = default);
}
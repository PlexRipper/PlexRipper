using FluentResults;

namespace Reaparr.FileSystem.Contracts;

public interface IMoveDownloadFileScheduler
{
    Task<Result> StartMoveDownloadFileJob(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken);

    Task<Result> StopMoveDownloadFileJob(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken);

    Task<bool> IsDownloadFileMoving(DownloadTaskKey downloadTaskKey, CancellationToken cancellationToken);

    Task<bool> IsAnyMoveDownloadFileJobRunning();

    Task<List<DownloadTaskKey>> GetCurrentlyMovingKeysByServer(int plexServerId);

    Task<List<DownloadTaskKey>> GetCurrentlyMovingKeysByServer(
        int plexServerId,
        CancellationToken cancellationToken
    ) => GetCurrentlyMovingKeysByServer(plexServerId);
}

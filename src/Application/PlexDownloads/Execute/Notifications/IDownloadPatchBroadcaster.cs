using Reaparr.Domain;

namespace Reaparr.Application;

public interface IDownloadPatchBroadcaster
{
    bool TryMarkStatusChanged(Guid nodeId, DownloadStatus status);

    Task MarkProgressDirtyAsync(
        int plexServerId,
        DownloadTaskKey rootKey,
        Guid changedNodeId,
        CancellationToken cancellationToken = default
    );

    Task PublishImmediateStatusPatchAsync(
        int plexServerId,
        DownloadTaskKey rootKey,
        IReadOnlyCollection<Guid> changedNodeIds,
        CancellationToken cancellationToken = default
    );
}

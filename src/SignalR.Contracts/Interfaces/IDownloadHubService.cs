namespace Reaparr.SignalR.Contracts;

public interface IDownloadHubService
{
    /// <summary>
    /// Sends a download progress update to the front-end.
    /// </summary>
    Task SendDownloadProgressUpdateAsync(
        List<DownloadTaskGeneric> downloadTasks,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sends a download patch update to the front-end.
    /// </summary>
    Task SendDownloadPatchAsync(
        int plexServerId,
        long sequence,
        IReadOnlyCollection<DownloadPatchDTO> upserts,
        IReadOnlyCollection<Guid>? deletedIds = null,
        CancellationToken cancellationToken = default
    );
}

using BackgroundServices.Contracts;
using PlexRipper.Domain;

namespace WebAPI.Contracts;

public interface ISignalRService
{
    Task SendLibraryProgressUpdateAsync(int id, int received, int total, bool isRefreshing = true);

    Task SendDownloadTaskUpdateAsync(DownloadTask downloadTask, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a <see cref="FileMergeProgress"/> object to the SignalR client in the front-end.
    /// </summary>
    /// <param name="fileMergeProgress">The <see cref="FileMergeProgress"/> object to send.</param>
    /// <param name="cancellationToken"></param>
    Task SendFileMergeProgressUpdateAsync(FileMergeProgress fileMergeProgress, CancellationToken cancellationToken = default);

    Task SendNotificationAsync(Notification notification);

    Task SendServerInspectStatusProgressAsync(InspectServerProgress progress);

    Task SendServerSyncProgressUpdateAsync(SyncServerProgress syncServerProgress);

    Task SendDownloadProgressUpdateAsync(int plexServerId, List<DownloadTask> downloadTasks, CancellationToken cancellationToken = default);

    Task SendServerConnectionCheckStatusProgressAsync(ServerConnectionCheckStatusProgress progress);
    Task SendJobStatusUpdateAsync(JobStatusUpdate jobStatusUpdate);
}
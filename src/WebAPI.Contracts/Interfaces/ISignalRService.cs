using BackgroundServices.Contracts;
using PlexRipper.Domain;

namespace WebAPI.Contracts;

public interface ISignalRService
{
    Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true);

    void SendDownloadTaskUpdate(DownloadTask downloadTask);

    /// <summary>
    /// Sends a <see cref="FileMergeProgress"/> object to the SignalR client in the front-end.
    /// </summary>
    /// <param name="fileMergeProgress">The <see cref="FileMergeProgress"/> object to send.</param>
    Task SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress);

    Task SendNotification(Notification notification);

    Task SendServerInspectStatusProgress(InspectServerProgress progress);

    Task SendServerSyncProgressUpdate(SyncServerProgress syncServerProgress);

    Task SendDownloadProgressUpdate(int plexServerId, List<DownloadTask> downloadTasks);

    Task SendServerConnectionCheckStatusProgress(ServerConnectionCheckStatusProgress progress);
    Task SendJobStatusUpdate(JobStatusUpdate jobStatusUpdate);
}
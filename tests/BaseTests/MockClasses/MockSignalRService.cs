using BackgroundServices.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.BaseTests;

public class MockSignalRService : ISignalRService
{
    public void SendLibraryProgressUpdate(LibraryProgress libraryProgress) { }

    public Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
    {
        return Task.CompletedTask;
    }

    public Task SendDownloadTaskCreationProgressUpdate(int current, int total)
    {
        return Task.CompletedTask;
    }

    public void SendDownloadTaskUpdate(DownloadTask downloadTask) { }

    public Task SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
    {
        return Task.CompletedTask;
    }

    public Task SendNotification(Notification notification)
    {
        return Task.CompletedTask;
    }

    public Task SendServerInspectStatusProgress(InspectServerProgress progress)
    {
        return Task.CompletedTask;
    }

    public Task SendServerSyncProgressUpdate(SyncServerProgress syncServerProgress)
    {
        return Task.CompletedTask;
    }

    public Task SendDownloadProgressUpdate(int plexServerId, List<DownloadTask> downloadTasks)
    {
        return Task.CompletedTask;
    }

    public Task SendServerConnectionCheckStatusProgress(ServerConnectionCheckStatusProgress progress)
    {
        return Task.CompletedTask;
    }

    public Task SendJobStatusUpdate(JobStatusUpdate jobStatusUpdate)
    {
        return Task.CompletedTask;
    }
}
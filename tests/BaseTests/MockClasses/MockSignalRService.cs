using PlexRipper.Application;

namespace PlexRipper.BaseTests;

public class MockSignalRService : ISignalRService
{
    public void SendLibraryProgressUpdate(LibraryProgress libraryProgress)
    {
    }

    public void SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
    {
    }

    public Task SendDownloadTaskCreationProgressUpdate(int current, int total)
    {
        return Task.CompletedTask;
    }

    public void SendDownloadTaskUpdate(DownloadTask downloadTask)
    {
    }

    public void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
    {
    }

    public Task SendNotification(Notification notification)
    {
        return Task.CompletedTask;
    }

    public Task SendServerInspectStatusProgress(InspectServerProgress progress)
    {
        return Task.CompletedTask;
    }

    public void SendServerSyncProgressUpdate(SyncServerProgress syncServerProgress)
    {
    }

    public Task SendDownloadProgressUpdate(int plexServerId, List<DownloadTask> downloadTasks)
    {
        return Task.CompletedTask;
    }
}
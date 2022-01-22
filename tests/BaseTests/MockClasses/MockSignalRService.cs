using System.Collections.Generic;
using System.Threading.Tasks;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class MockSignalRService : ISignalRService
    {
        public void SendLibraryProgressUpdate(LibraryProgress libraryProgress)
        {
        }

        public void SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true)
        {
        }

        public async Task SendDownloadTaskCreationProgressUpdate(int current, int total)
        {

        }

        public void SendDownloadTaskUpdate(DownloadTask downloadTask)
        {
        }

        public void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress)
        {
        }

        public async Task SendNotification(Notification notification)
        {
        }

        public async Task SendServerInspectStatusProgress(InspectServerProgress progress)
        {
        }

        public void SendServerSyncProgressUpdate(SyncServerProgress syncServerProgress)
        {
        }

        public async Task SendDownloadProgressUpdate(int plexServerId, List<DownloadTask> downloadTasks)
        {
        }
    }
}
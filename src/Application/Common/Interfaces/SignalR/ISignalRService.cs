using System.Threading.Tasks;
using PlexRipper.Application.Common.DTO.DownloadManager;
using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface ISignalRService
    {
        Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true);

        Task SendDownloadTaskCreationProgressUpdate(int current, int total);

        Task SendDownloadTaskUpdate(DownloadClientUpdate downloadClientUpdate);

        /// <summary>
        /// Sends a <see cref="FileMergeProgress"/> object to the SignalR client in the front-end.
        /// </summary>
        /// <param name="fileMergeProgress">The <see cref="FileMergeProgress"/> object to send.</param>
        void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress);

        Task SendNotification(Notification notification);

        Task SendPlexAccountRefreshUpdate(int plexAccountId, int received, int total, bool isRefreshing = true);
    }
}
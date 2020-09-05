using System.Threading.Tasks;
using PlexRipper.Domain.Enums;
using PlexRipper.Domain.Types.FileSystem;

namespace PlexRipper.Application.Common
{
    public interface ISignalRService
    {
        Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true);
        Task SendDownloadTaskCreationProgressUpdate(int plexLibraryId, int current, int total);
        Task SendDownloadProgressUpdate(IDownloadProgress downloadProgress);
        Task SendDownloadStatusUpdate(int id, DownloadStatus downloadStatus);

        /// <summary>
        /// Sends a <see cref="FileMergeProgress"/> object to the SignalR client in the front-end.
        /// </summary>
        /// <param name="fileMergeProgress">The <see cref="FileMergeProgress"/> object to send.</param>
        void SendFileMergeProgressUpdate(FileMergeProgress fileMergeProgress);
    }
}
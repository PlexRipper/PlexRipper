using System.Threading.Tasks;
using PlexRipper.Domain.Enums;

namespace PlexRipper.Application.Common
{
    public interface ISignalRService
    {
        Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true);
        Task SendDownloadTaskCreationProgressUpdate(int plexLibraryId, int current, int total);
        Task SendDownloadProgressUpdate(IDownloadProgress downloadProgress);
        Task SendDownloadStatusUpdate(int id, DownloadStatus downloadStatus);
    }
}
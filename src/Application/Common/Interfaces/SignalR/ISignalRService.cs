using System.Threading.Tasks;
using PlexRipper.Application.Common.Interfaces.DownloadManager;
using PlexRipper.Domain.Types;

namespace PlexRipper.Application.Common.Interfaces.SignalR
{
    public interface ISignalRService
    {
        Task SendLibraryProgressUpdate(int id, int received, int total, bool isRefreshing = true);
        Task SendDownloadTaskCreationProgressUpdate(int plexLibraryId, int current, int total);
        Task SendDownloadProgressUpdate(IDownloadProgress downloadProgress);
    }
}
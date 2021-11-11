using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadProgressNotifier
    {
        Task<Result> SendDownloadProgress();

        Task SendDownloadProgress(int plexServerId);
    }
}
using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadProgressScheduler
    {
        Task<Result> StartDownloadProgressJob(int plexServerId);

        Task<Result> StopDownloadProgressJob(int plexServerId);
    }
}
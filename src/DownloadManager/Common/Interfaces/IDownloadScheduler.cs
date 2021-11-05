using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.DownloadManager
{
    public interface IDownloadScheduler
    {
        Task<Result> StartDownloadJob(int downloadTaskId);
    }
}
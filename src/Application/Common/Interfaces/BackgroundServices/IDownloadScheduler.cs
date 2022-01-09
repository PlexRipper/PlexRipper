using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.Application
{
    public interface IDownloadScheduler : IBaseScheduler
    {
        Task<Result> StartDownloadJob(int downloadTaskId);
    }
}
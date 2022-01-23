using System.Threading.Tasks;
using FluentResults;

namespace PlexRipper.Application
{
    public interface ISchedulerService : IBaseScheduler
    {
        Task<Result> TriggerSyncPlexServersJob();

        Task InspectPlexServersAsyncJob(int plexAccountId);
    }
}
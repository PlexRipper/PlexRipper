using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface ISchedulerService : IBaseScheduler
    {
        Task<Result> TriggerSyncPlexServersJob();

        Task InspectPlexServersAsyncJob(int plexAccountId);
    }
}
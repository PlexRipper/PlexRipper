using System.Collections.Generic;
using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface ISchedulerService : ISetupAsync
    {
        Task<Result> TriggerSyncPlexServersJob();

        Task InspectPlexServersAsyncJob(int plexAccountId);
    }
}
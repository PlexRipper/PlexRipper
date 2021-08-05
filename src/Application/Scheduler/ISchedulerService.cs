using System.Threading.Tasks;
using FluentResults;
using PlexRipper.Domain;

namespace PlexRipper.Application
{
    public interface ISchedulerService : ISetup
    {
        Task<Result> TriggerSyncPlexServersJob();
    }
}
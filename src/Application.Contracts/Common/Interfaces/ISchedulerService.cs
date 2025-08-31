using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public interface ISchedulerService : ISetupAsync, IStopAsync
{
    Task AwaitScheduler(CancellationToken cancellationToken = default);

    Task<List<JobStatusUpdate<string>>> GetRunningJobUpdates();
}

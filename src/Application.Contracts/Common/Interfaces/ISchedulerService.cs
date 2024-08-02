using PlexRipper.Domain;

namespace Application.Contracts;

public interface ISchedulerService : ISetupAsync, IStopAsync
{
    Task AwaitScheduler(CancellationToken cancellationToken = default);
}

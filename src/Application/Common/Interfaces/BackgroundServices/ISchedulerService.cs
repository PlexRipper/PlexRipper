namespace PlexRipper.Application;

public interface ISchedulerService : IBaseScheduler
{
    Task AwaitScheduler(CancellationToken cancellationToken = default);
}
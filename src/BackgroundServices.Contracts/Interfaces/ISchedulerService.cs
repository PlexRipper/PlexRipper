namespace BackgroundServices.Contracts;

public interface ISchedulerService : IBaseScheduler
{
    Task AwaitScheduler(CancellationToken cancellationToken = default);
}
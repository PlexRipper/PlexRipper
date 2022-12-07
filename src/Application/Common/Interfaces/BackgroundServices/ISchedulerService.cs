namespace PlexRipper.Application;

public interface ISchedulerService : IBaseScheduler
{
    Task<Result> TriggerSyncPlexServersJob();

    Task QueueInspectPlexServersJobAsync(int plexAccountId);
}
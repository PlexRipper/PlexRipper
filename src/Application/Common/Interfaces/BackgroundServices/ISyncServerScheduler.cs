namespace BackgroundServices.SyncServer;

public interface ISyncServerScheduler
{
    Task<Result> QueueSyncPlexServersJob(int plexServerId);
}
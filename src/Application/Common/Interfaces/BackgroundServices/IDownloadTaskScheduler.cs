namespace PlexRipper.Application;

public interface IDownloadTaskScheduler
{
    Task<Result> StartDownloadTaskJob(int downloadTaskId, int plexServerId);
    Task<Result> StopDownloadTaskJob(int downloadTaskId);
    Task<bool> IsDownloading(int downloadTaskId);
    Task<bool> IsServerDownloading(int plexServerId);
}
namespace PlexRipper.Application;

public interface IDownloadTaskScheduler
{
    Task<Result> StartDownloadTaskJob(int downloadTaskId);
    Task<Result> StopDownloadTaskJob(int downloadTaskId);
}
namespace PlexRipper.Application;

public interface IDownloadTaskScheduler
{
    Task<Result> StartDownloadTaskJob(int downloadTaskId);
}
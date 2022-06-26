using FluentResults;

namespace PlexRipper.Application
{
    public interface IDownloadProgressScheduler : IBaseScheduler
    {
        Task<Result> StartDownloadProgressJob(int plexServerId);

        Task<Result> StopDownloadProgressJob(int plexServerId);

        Task<Result> TrackDownloadProgress(int plexServerId, string hashCode);

        Task<Result> FireDownloadProgressJob(int plexServerId);
    }
}
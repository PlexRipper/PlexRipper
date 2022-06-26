namespace PlexRipper.DownloadManager
{
    public interface IDownloadProgressNotifier
    {
        Task<Result<string>> SendDownloadProgress(int plexServerId);
    }
}
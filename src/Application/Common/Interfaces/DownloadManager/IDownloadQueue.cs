using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface IDownloadQueue : ISetup
    {
        void CheckDownloadQueue(IDownloadManager downloadManager);
    }
}
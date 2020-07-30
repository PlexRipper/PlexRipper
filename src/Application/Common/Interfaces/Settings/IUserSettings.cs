using PlexRipper.Application.Common.Interfaces.DownloadManager;

namespace PlexRipper.Application.Common.Interfaces.Settings
{
    public interface IUserSettings
    {
        bool Save();
        bool Load();
        int ActiveAccountId { get; set; }
        bool ConfirmExit { get; set; }

        IDownloadManagerModel DownloadManager { get; set; }
        void Reset();
    }
}

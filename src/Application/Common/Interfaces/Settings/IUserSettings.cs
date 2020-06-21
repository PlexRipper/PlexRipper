namespace PlexRipper.Application.Common.Interfaces.Settings
{
    public interface IUserSettings
    {
        bool Save();
        bool Load();
        string ApiKey { get; set; }
        bool ConfirmExit { get; set; }

        IDownloadManagerModel DownloadManager { get; set; }
        void Reset();
    }
}

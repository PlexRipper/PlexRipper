namespace PlexRipper.Application.Common
{
    public interface IAdvancedSettingsModel
    {
        IDownloadManagerModel DownloadManager { get; set; }
    }
}
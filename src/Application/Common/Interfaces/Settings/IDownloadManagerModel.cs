namespace PlexRipper.Application.Common.Interfaces.Settings
{
    public interface IDownloadManagerModel
    {
        int MaxDownloads { get; set; }
        bool EnableSpeedLimit { get; set; }
        int SpeedLimit { get; set; }
        bool StartDownloadsOnStartup { get; set; }
        bool ConfirmDelete { get; set; }
        string DownloadLocation { get; set; }
        int MemoryCacheSize { get; set; }
        bool ManualProxyConfig { get; set; }
        string HttpProxy { get; set; }
        int ProxyPort { get; set; }
        string ProxyUsername { get; set; }
        string ProxyPassword { get; set; }
    }
}

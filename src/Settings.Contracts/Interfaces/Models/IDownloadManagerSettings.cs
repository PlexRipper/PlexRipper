namespace Reaparr.Settings.Contracts;

public interface IDownloadManagerSettings
{
    int DownloadSegments { get; set; }
    bool KeepCompletedInDownloadFolder { get; set; }
}

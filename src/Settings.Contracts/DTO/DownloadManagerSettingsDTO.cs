namespace Settings.Contracts;

public class DownloadManagerSettingsDTO : IDownloadManagerSettings
{
    public int DownloadSegments { get; set; }
}
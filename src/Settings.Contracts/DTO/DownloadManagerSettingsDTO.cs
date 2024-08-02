namespace Settings.Contracts;

public class DownloadManagerSettingsDTO : IDownloadManagerSettings
{
    public required int DownloadSegments { get; set; }
}

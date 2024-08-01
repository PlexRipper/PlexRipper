using Settings.Contracts;

namespace PlexRipper.Settings;

public class DownloadManagerSettings : IDownloadManagerSettings
{
    public int DownloadSegments { get; set; }
}

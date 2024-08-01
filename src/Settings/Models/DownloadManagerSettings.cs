using Settings.Contracts;

namespace PlexRipper.Settings;

public record DownloadManagerSettings : IDownloadManagerSettings
{
    public int DownloadSegments { get; set; } = 4;
}

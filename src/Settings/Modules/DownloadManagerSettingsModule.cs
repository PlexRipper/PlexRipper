using Settings.Contracts;

namespace PlexRipper.Settings;

public class DownloadManagerSettingsModule
    : BaseSettingsModule<IDownloadManagerSettings>,
        IDownloadManagerSettingsModule
{
    public int DownloadSegments { get; set; }

    public override string Name => "DownloadManagerSettings";

    public override IDownloadManagerSettings DefaultValues() => new DownloadManagerSettings { DownloadSegments = 4 };

    public override IDownloadManagerSettings GetValues() =>
        new DownloadManagerSettings { DownloadSegments = DownloadSegments };
}

using PlexRipper.Application;
using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public class DownloadManagerSettingsModule : BaseSettingsModule<IDownloadManagerSettings>, IDownloadManagerSettingsModule
{
    public int DownloadSegments { get; set; }

    public override string Name => "DownloadManagerSettings";

    public override IDownloadManagerSettings DefaultValues()
    {
        return new DownloadManagerSettings
        {
            DownloadSegments = 4,
        };
    }

    public override IDownloadManagerSettings GetValues()
    {
        return new DownloadManagerSettings
        {
            DownloadSegments = DownloadSegments,
        };
    }
}
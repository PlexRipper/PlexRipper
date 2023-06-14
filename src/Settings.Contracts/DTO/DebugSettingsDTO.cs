using PlexRipper.Domain.DownloadManager;

namespace Settings.Contracts;

public class DebugSettingsDTO : IDebugSettings
{
    public bool DebugModeEnabled { get; set; }
    public bool MaskServerNames { get; set; }
    public bool MaskLibraryNames { get; set; }
}
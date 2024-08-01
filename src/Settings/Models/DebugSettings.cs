using Settings.Contracts;

namespace PlexRipper.Settings;

public record DebugSettings : IDebugSettings
{
    public bool DebugModeEnabled { get; set; } = false;

    public bool MaskServerNames { get; set; } = false;

    public bool MaskLibraryNames { get; set; } = false;
}

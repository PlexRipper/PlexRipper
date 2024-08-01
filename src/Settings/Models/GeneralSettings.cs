using Settings.Contracts;

namespace PlexRipper.Settings;

public record GeneralSettings : IGeneralSettings
{
    public bool FirstTimeSetup { get; set; } = true;

    public int ActiveAccountId { get; set; } = 0;
    public bool DebugMode { get; set; } = false;

    public bool DisableAnimatedBackground { get; set; } = false;
}

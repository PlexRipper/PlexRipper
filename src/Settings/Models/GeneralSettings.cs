using Settings.Contracts;

namespace PlexRipper.Settings;

public class GeneralSettings : IGeneralSettings
{
    public bool FirstTimeSetup { get; set; }

    public int ActiveAccountId { get; set; }
    public bool DebugMode { get; set; }

    public bool DisableAnimatedBackground { get; set; }
}

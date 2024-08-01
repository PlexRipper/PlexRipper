using Settings.Contracts;

namespace PlexRipper.Settings;

public class DebugSettings : IDebugSettings
{
    #region Properties

    public bool DebugModeEnabled { get; set; }

    public bool MaskServerNames { get; set; }

    public bool MaskLibraryNames { get; set; }

    #endregion
}

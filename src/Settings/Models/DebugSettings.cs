using Settings.Contracts;

namespace PlexRipper.Settings.Models;

public class DebugSettings : IDebugSettings
{
    #region Properties

    public bool DebugModeEnabled { get; set; }

    public bool MaskServerNames { get; set; }

    public bool MaskLibraryNames { get; set; }

    #endregion
}

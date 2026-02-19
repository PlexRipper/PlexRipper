namespace Reaparr.Settings.Contracts;

public interface IDebugSettings
{
    #region Properties

    bool DebugModeEnabled { get; set; }

    bool MaskServerNames { get; set; }

    bool MaskLibraryNames { get; set; }

    bool MaskAccountNames { get; set; }

    #endregion
}

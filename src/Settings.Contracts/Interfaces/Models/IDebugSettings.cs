namespace Settings.Contracts;

public interface IDebugSettings
{
    #region Properties

    bool DebugModeEnabled { get; set; }

    bool MaskServerNames { get; set; }

    bool MaskLibraryNames { get; set; }

    #endregion
}

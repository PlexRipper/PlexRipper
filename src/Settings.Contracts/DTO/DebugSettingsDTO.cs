namespace Settings.Contracts;

public class DebugSettingsDTO : IDebugSettings
{
    public required bool DebugModeEnabled { get; set; }

    public required bool MaskServerNames { get; set; }

    public required bool MaskLibraryNames { get; set; }
}

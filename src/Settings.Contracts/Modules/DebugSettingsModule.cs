namespace Reaparr.Settings.Contracts;

public record DebugSettingsModule
    : BaseSettingsModule<DebugSettingsModule>,
        IBaseSettingsModule<DebugSettingsModule>,
        IDebugSettings
{
    public static DebugSettingsModule Create() =>
        new()
        {
            DebugModeEnabled = false,
            MaskServerNames = false,
            MaskLibraryNames = false,
        };

    public required bool DebugModeEnabled
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool MaskServerNames
    {
        get;
        set => SetProperty(ref field, value);
    }

    public required bool MaskLibraryNames
    {
        get;
        set => SetProperty(ref field, value);
    }
}

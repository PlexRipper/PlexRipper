namespace Reaparr.Settings.Contracts;

public record DebugSettingsModule : BaseSettingsModule<DebugSettingsModule>, IDebugSettings
{
    private bool _debugModeEnabled;
    private bool _maskServerNames;
    private bool _maskLibraryNames;

    public static DebugSettingsModule Create() =>
        new()
        {
            DebugModeEnabled = false,
            MaskServerNames = false,
            MaskLibraryNames = false,
        };

    public required bool DebugModeEnabled
    {
        get => _debugModeEnabled;
        set => SetProperty(ref _debugModeEnabled, value);
    }

    public required bool MaskServerNames
    {
        get => _maskServerNames;
        set => SetProperty(ref _maskServerNames, value);
    }

    public required bool MaskLibraryNames
    {
        get => _maskLibraryNames;
        set => SetProperty(ref _maskLibraryNames, value);
    }
}

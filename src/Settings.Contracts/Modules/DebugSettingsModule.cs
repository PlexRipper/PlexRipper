namespace Settings.Contracts;

public record DebugSettingsModule : BaseSettingsModule<DebugSettingsModule>, IDebugSettings
{
    private bool _debugModeEnabled = false;
    private bool _maskServerNames = false;
    private bool _maskLibraryNames = false;

    public bool DebugModeEnabled
    {
        get => _debugModeEnabled;
        set => SetProperty(ref _debugModeEnabled, value);
    }

    public bool MaskServerNames
    {
        get => _maskServerNames;
        set => SetProperty(ref _maskServerNames, value);
    }

    public bool MaskLibraryNames
    {
        get => _maskLibraryNames;
        set => SetProperty(ref _maskLibraryNames, value);
    }
}

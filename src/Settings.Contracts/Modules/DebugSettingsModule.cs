namespace Settings.Contracts;

public record DebugSettingsModule : BaseSettingsModule<DebugSettingsModule>, IDebugSettings
{
    private bool _debugModeEnabled;
    private bool _maskServerNames;
    private bool _maskLibraryNames;

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

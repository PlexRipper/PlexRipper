namespace Settings.Contracts;

public record GeneralSettingsModule : BaseSettingsModule<GeneralSettingsModule>, IGeneralSettings
{
    private bool _firstTimeSetup = true;
    private int _activeAccountId;
    private bool _debugMode;
    private bool _disableAnimatedBackground;

    public bool FirstTimeSetup
    {
        get => _firstTimeSetup;
        set => SetProperty(ref _firstTimeSetup, value);
    }

    public int ActiveAccountId
    {
        get => _activeAccountId;
        set => SetProperty(ref _activeAccountId, value);
    }

    public bool DebugMode
    {
        get => _debugMode;
        set => SetProperty(ref _debugMode, value);
    }

    public bool DisableAnimatedBackground
    {
        get => _disableAnimatedBackground;
        set => SetProperty(ref _disableAnimatedBackground, value);
    }
}

namespace Settings.Contracts;

public record GeneralSettingsModel : BaseSettingsModel<GeneralSettingsModel>, IGeneralSettings
{
    private bool _firstTimeSetup = true;
    private int _activeAccountId = 0;
    private bool _debugMode = false;
    private bool _disableAnimatedBackground = false;

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

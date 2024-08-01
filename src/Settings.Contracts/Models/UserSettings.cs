using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json.Serialization;

namespace Settings.Contracts;

/// <inheritdoc cref="IUserSettings"/>
public class UserSettings : IUserSettings
{
    public GeneralSettingsModel GeneralSettings
    {
        get => _generalSettings;
        init => _generalSettings = value;
    }

    public ConfirmationSettingsModel ConfirmationSettings
    {
        get => _confirmationSettings;
        init => _confirmationSettings = value;
    }

    public DateTimeSettingsModel DateTimeSettings
    {
        get => _dateTimeSettings;
        init => _dateTimeSettings = value;
    }

    public DisplaySettingsModel DisplaySettings
    {
        get => _displaySettings;
        init => _displaySettings = value;
    }

    public DownloadManagerSettingsModel DownloadManagerSettings
    {
        get => _downloadManagerSettings;
        init => _downloadManagerSettings = value;
    }

    public LanguageSettingsModel LanguageSettings
    {
        get => _languageSettings;
        init => _languageSettings = value;
    }

    public DebugSettingsModel DebugSettings
    {
        get => _debugSettings;
        init => _debugSettings = value;
    }

    public ServerSettingsModel ServerSettings
    {
        get => _serverSettings;
        init => _serverSettings = value;
    }

    private readonly Subject<UserSettings> _settingsUpdated = new();

    private GeneralSettingsModel _generalSettings = new();
    private ConfirmationSettingsModel _confirmationSettings = new();
    private DateTimeSettingsModel _dateTimeSettings = new();
    private DisplaySettingsModel _displaySettings = new();
    private DownloadManagerSettingsModel _downloadManagerSettings = new();
    private LanguageSettingsModel _languageSettings = new();
    private DebugSettingsModel _debugSettings = new();
    private ServerSettingsModel _serverSettings = new() { Data = [] };

    /// <summary>
    /// The <see cref="UserSettings"/> class is a wrapper class for the individual Settings.
    /// </summary>
    public UserSettings()
    {
        // Alert of any  changes
        Observable
            .Merge(
                ConfirmationSettings.HasChanged.Select(_ => 1),
                DateTimeSettings.HasChanged.Select(_ => 1),
                DisplaySettings.HasChanged.Select(_ => 1),
                DownloadManagerSettings.HasChanged.Select(_ => 1),
                GeneralSettings.HasChanged.Select(_ => 1),
                DebugSettings.HasChanged.Select(_ => 1),
                LanguageSettings.HasChanged.Select(_ => 1),
                ServerSettings.HasChanged.Select(_ => 1)
            )
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => _settingsUpdated.OnNext(this));
    }

    [JsonIgnore]
    public IObservable<UserSettings> SettingsUpdated => _settingsUpdated.AsObservable();

    public void Reset()
    {
        _confirmationSettings = new ConfirmationSettingsModel();
        _dateTimeSettings = new DateTimeSettingsModel();
        _displaySettings = new DisplaySettingsModel();
        _downloadManagerSettings = new DownloadManagerSettingsModel();
        _generalSettings = new GeneralSettingsModel();
        _debugSettings = new DebugSettingsModel();
        _languageSettings = new LanguageSettingsModel();
        _serverSettings = new ServerSettingsModel();
    }

    public UserSettings UpdateSettings(ISettingsModel sourceSettings)
    {
        _confirmationSettings = sourceSettings.ConfirmationSettings;
        _dateTimeSettings = sourceSettings.DateTimeSettings;
        _displaySettings = sourceSettings.DisplaySettings;
        _downloadManagerSettings = sourceSettings.DownloadManagerSettings;
        _generalSettings = sourceSettings.GeneralSettings;
        _debugSettings = sourceSettings.DebugSettings;
        _languageSettings = sourceSettings.LanguageSettings;
        _serverSettings = sourceSettings.ServerSettings;

        return this;
    }
}

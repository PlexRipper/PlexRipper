using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text.Json;
using Logging.Interface;
using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings;

/// <inheritdoc cref="IUserSettings"/>
public class UserSettings : IUserSettings
{
    #region Fields

    private readonly ILog _log;
    private readonly IConfirmationSettingsModule _confirmationSettingsModule;

    private readonly IDateTimeSettingsModule _dateTimeSettingsModule;

    private readonly IDisplaySettingsModule _displaySettingsModule;

    private readonly IDownloadManagerSettingsModule _downloadManagerSettingsModule;

    private readonly IGeneralSettingsModule _generalSettingsModule;

    private readonly IDebugSettingsModule _debugSettingsModule;

    private readonly ILanguageSettingsModule _languageSettingsModule;

    private readonly IServerSettingsModule _serverSettingsModule;

    private readonly Subject<ISettingsModel> _settingsUpdated = new();

    #endregion

    /// <summary>
    /// The <see cref="UserSettings"/> class is a wrapper class for the individual SettingsModules.
    /// </summary>
    /// <param name="confirmationSettingsModule"></param>
    /// <param name="dateTimeSettingsModule"></param>
    /// <param name="displaySettingsModule"></param>
    /// <param name="downloadManagerSettingsModule"></param>
    /// <param name="generalSettingsModule"></param>
    /// <param name="debugSettingsModule"></param>
    /// <param name="languageSettingsModule"></param>
    /// <param name="serverSettingsModule"></param>
    public UserSettings(
        ILog log,
        IConfirmationSettingsModule confirmationSettingsModule,
        IDateTimeSettingsModule dateTimeSettingsModule,
        IDisplaySettingsModule displaySettingsModule,
        IDownloadManagerSettingsModule downloadManagerSettingsModule,
        IGeneralSettingsModule generalSettingsModule,
        IDebugSettingsModule debugSettingsModule,
        ILanguageSettingsModule languageSettingsModule,
        IServerSettingsModule serverSettingsModule
    )
    {
        _log = log;
        _confirmationSettingsModule = confirmationSettingsModule;
        _dateTimeSettingsModule = dateTimeSettingsModule;
        _displaySettingsModule = displaySettingsModule;
        _downloadManagerSettingsModule = downloadManagerSettingsModule;
        _generalSettingsModule = generalSettingsModule;
        _debugSettingsModule = debugSettingsModule;
        _languageSettingsModule = languageSettingsModule;
        _serverSettingsModule = serverSettingsModule;

        // Alert of any module changes
        Observable
            .Merge(
                _confirmationSettingsModule.ModuleHasChanged.Select(_ => 1),
                _dateTimeSettingsModule.ModuleHasChanged.Select(_ => 1),
                _displaySettingsModule.ModuleHasChanged.Select(_ => 1),
                _downloadManagerSettingsModule.ModuleHasChanged.Select(_ => 1),
                _generalSettingsModule.ModuleHasChanged.Select(_ => 1),
                _debugSettingsModule.ModuleHasChanged.Select(_ => 1),
                _languageSettingsModule.ModuleHasChanged.Select(_ => 1),
                _serverSettingsModule.ModuleHasChanged.Select(_ => 1)
            )
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => _settingsUpdated.OnNext(GetSettingsModel()));
    }

    #region Methods

    #region Public

    public IObservable<ISettingsModel> SettingsUpdated => _settingsUpdated.AsObservable();

    public void Reset()
    {
        _log.InformationLine("Resetting UserSettings");

        _confirmationSettingsModule.Reset();
        _dateTimeSettingsModule.Reset();
        _displaySettingsModule.Reset();
        _downloadManagerSettingsModule.Reset();
        _generalSettingsModule.Reset();
        _debugSettingsModule.Reset();
        _languageSettingsModule.Reset();
        _serverSettingsModule.Reset();
    }

    public Result<ISettingsModel> UpdateSettings(ISettingsModel sourceSettings)
    {
        var settings = new SettingsModel
        {
            ConfirmationSettings = _confirmationSettingsModule.Update(sourceSettings.ConfirmationSettings),
            DateTimeSettings = _dateTimeSettingsModule.Update(sourceSettings.DateTimeSettings),
            DisplaySettings = _displaySettingsModule.Update(sourceSettings.DisplaySettings),
            DownloadManagerSettings = _downloadManagerSettingsModule.Update(sourceSettings.DownloadManagerSettings),
            GeneralSettings = _generalSettingsModule.Update(sourceSettings.GeneralSettings),
            LanguageSettings = _languageSettingsModule.Update(sourceSettings.LanguageSettings),
            ServerSettings = _serverSettingsModule.Update(sourceSettings.ServerSettings),
            DebugSettings = _debugSettingsModule.Update(sourceSettings.DebugSettings),
        };

        return Result.Ok((ISettingsModel)settings);
    }

    /// <summary>
    /// Creates a <see cref="SettingsModel"/> with all the current set values.
    /// </summary>
    /// <returns></returns>
    public ISettingsModel GetSettingsModel()
    {
        return new SettingsModel
        {
            ConfirmationSettings = _confirmationSettingsModule.GetValues(),
            GeneralSettings = _generalSettingsModule.GetValues(),
            DisplaySettings = _displaySettingsModule.GetValues(),
            LanguageSettings = _languageSettingsModule.GetValues(),
            ServerSettings = _serverSettingsModule.GetValues(),
            DateTimeSettings = _dateTimeSettingsModule.GetValues(),
            DownloadManagerSettings = _downloadManagerSettingsModule.GetValues(),
            DebugSettings = _debugSettingsModule.GetValues(),
        };
    }

    /// <summary>
    /// Creates a <see cref="SettingsModel"/> with all the default values.
    /// </summary>
    /// <returns></returns>
    public ISettingsModel GetDefaultSettingsModel()
    {
        return new SettingsModel
        {
            ConfirmationSettings = _confirmationSettingsModule.DefaultValues(),
            GeneralSettings = _generalSettingsModule.DefaultValues(),
            DisplaySettings = _displaySettingsModule.DefaultValues(),
            LanguageSettings = _languageSettingsModule.DefaultValues(),
            ServerSettings = _serverSettingsModule.DefaultValues(),
            DateTimeSettings = _dateTimeSettingsModule.DefaultValues(),
            DownloadManagerSettings = _downloadManagerSettingsModule.DefaultValues(),
            DebugSettings = _debugSettingsModule.DefaultValues(),
        };
    }

    public Result SetFromJsonObject(JsonElement settingsJsonElement)
    {
        var results = new List<Result>
        {
            _confirmationSettingsModule.SetFromJson(settingsJsonElement),
            _dateTimeSettingsModule.SetFromJson(settingsJsonElement),
            _displaySettingsModule.SetFromJson(settingsJsonElement),
            _downloadManagerSettingsModule.SetFromJson(settingsJsonElement),
            _generalSettingsModule.SetFromJson(settingsJsonElement),
            _languageSettingsModule.SetFromJson(settingsJsonElement),
            _serverSettingsModule.SetFromJson(settingsJsonElement),
            _debugSettingsModule.SetFromJson(settingsJsonElement),
        };

        if (results.Any(x => x.IsFailed))
            return Result
                .Fail("Failed to set from json object")
                .AddNestedErrors(results.SelectMany(x => x.Errors).ToList());

        return Result.Ok();
    }

    #endregion

    #endregion
}

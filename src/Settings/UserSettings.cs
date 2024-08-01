using System.Reactive.Linq;
using System.Reactive.Subjects;
using Logging.Interface;
using Settings.Contracts;

namespace PlexRipper.Settings;

/// <inheritdoc cref="IUserSettings"/>
public class UserSettings : IUserSettings
{
    #region Fields

    private readonly ILog _log;

    public ConfirmationSettingsModel ConfirmationSettingsModel { get; private set; } = new();

    public DateTimeSettingsModel DateTimeSettingsModel { get; private set; } = new();

    public DisplaySettingsModel DisplaySettingsModel { get; private set; } = new();

    public DownloadManagerSettingsModel DownloadManagerSettingsModel { get; private set; } = new();

    public GeneralSettingsModel GeneralSettingsModel { get; private set; } = new();

    public DebugSettingsModel DebugSettingsModel { get; private set; } = new();

    public LanguageSettingsModel LanguageSettingsModel { get; private set; } = new();

    public ServerSettingsModel ServerSettingsModel { get; private set; } = new();

    private readonly Subject<SettingsModel> _settingsUpdated = new();

    #endregion

    /// <summary>
    /// The <see cref="UserSettings"/> class is a wrapper class for the individual Settings.
    /// </summary>
    public UserSettings(ILog log)
    {
        _log = log;

        // Alert of any  changes
        Observable
            .Merge(
                ConfirmationSettingsModel.HasChanged.Select(_ => 1),
                DateTimeSettingsModel.HasChanged.Select(_ => 1),
                DisplaySettingsModel.HasChanged.Select(_ => 1),
                DownloadManagerSettingsModel.HasChanged.Select(_ => 1),
                GeneralSettingsModel.HasChanged.Select(_ => 1),
                DebugSettingsModel.HasChanged.Select(_ => 1),
                LanguageSettingsModel.HasChanged.Select(_ => 1),
                ServerSettingsModel.HasChanged.Select(_ => 1)
            )
            .Throttle(TimeSpan.FromMilliseconds(500))
            .Subscribe(_ => _settingsUpdated.OnNext(GetSettingsModel()));
    }

    #region Methods

    #region Public

    public IObservable<SettingsModel> SettingsUpdated => _settingsUpdated.AsObservable();

    public void Reset()
    {
        _log.InformationLine("Resetting UserSettings");

        ConfirmationSettingsModel = new ConfirmationSettingsModel();
        DateTimeSettingsModel = new DateTimeSettingsModel();
        DisplaySettingsModel = new DisplaySettingsModel();
        DownloadManagerSettingsModel = new DownloadManagerSettingsModel();
        GeneralSettingsModel = new GeneralSettingsModel();
        DebugSettingsModel = new DebugSettingsModel();
        LanguageSettingsModel = new LanguageSettingsModel();
        ServerSettingsModel = new ServerSettingsModel();
    }

    public SettingsModel UpdateSettings(SettingsModel sourceSettings)
    {
        ConfirmationSettingsModel = sourceSettings.ConfirmationSettingsModel;
        DateTimeSettingsModel = sourceSettings.DateTimeSettingsModel;
        DisplaySettingsModel = sourceSettings.DisplaySettingsModel;
        DownloadManagerSettingsModel = sourceSettings.DownloadManagerSettingsModel;
        DebugSettingsModel = sourceSettings.DebugSettingsModel;
        GeneralSettingsModel = sourceSettings.GeneralSettingsModel;
        LanguageSettingsModel = sourceSettings.LanguageSettingsModel;
        ServerSettingsModel = sourceSettings.ServerSettingsModel;

        return GetSettingsModel();
    }

    /// <summary>
    /// Creates a <see cref="SettingsModule"/> with all the current set values.
    /// </summary>
    /// <returns></returns>
    public SettingsModel GetSettingsModel() =>
        new()
        {
            ConfirmationSettingsModel = ConfirmationSettingsModel,
            GeneralSettingsModel = GeneralSettingsModel,
            DisplaySettingsModel = DisplaySettingsModel,
            LanguageSettingsModel = LanguageSettingsModel,
            ServerSettingsModel = ServerSettingsModel,
            DateTimeSettingsModel = DateTimeSettingsModel,
            DownloadManagerSettingsModel = DownloadManagerSettingsModel,
            DebugSettingsModel = DebugSettingsModel,
        };

    #endregion

    #endregion
}

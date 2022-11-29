using PlexRipper.Application;
using PlexRipper.Settings.Modules;

namespace PlexRipper.Settings.Models;

/// <summary>
/// Used to model the settings, which is then used to serialize to json.
/// </summary>
public class SettingsModel : ISettingsModel
{
    #region Properties

    public IGeneralSettings GeneralSettings { get; set; }

    public IConfirmationSettings ConfirmationSettings { get; set; }

    public IDateTimeSettings DateTimeSettings { get; set; }

    public IDisplaySettings DisplaySettings { get; set; }

    public IDownloadManagerSettings DownloadManagerSettings { get; set; }

    public ILanguageSettings LanguageSettings { get; set; }

    public IServerSettings ServerSettings { get; set; }

    #endregion

    public static SettingsModel DefaultSettings()
    {
        return new SettingsModel()
        {
            GeneralSettings = new GeneralSettingsModule(),
            ConfirmationSettings = new ConfirmationSettingsModule(),
            DateTimeSettings = new DateTimeSettingsModule(),
            DisplaySettings = new DisplaySettingsModule(),
            DownloadManagerSettings = new DownloadManagerSettingsModule(),
            LanguageSettings = new LanguageSettingsModule(),
            ServerSettings = new ServerSettingsModule(),
        };
    }
}
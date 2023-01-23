namespace Settings.Contracts;

public interface ISettingsModel
{
    IGeneralSettings GeneralSettings { get; set; }

    IConfirmationSettings ConfirmationSettings { get; set; }

    IDateTimeSettings DateTimeSettings { get; set; }

    IDisplaySettings DisplaySettings { get; set; }

    IDownloadManagerSettings DownloadManagerSettings { get; set; }

    ILanguageSettings LanguageSettings { get; set; }

    IServerSettings ServerSettings { get; set; }
}
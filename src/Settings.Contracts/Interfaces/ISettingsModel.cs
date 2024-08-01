namespace Settings.Contracts;

public interface ISettingsModel
{
    GeneralSettingsModel GeneralSettings { get; init; }

    ConfirmationSettingsModel ConfirmationSettings { get; init; }

    DateTimeSettingsModel DateTimeSettings { get; init; }

    DisplaySettingsModel DisplaySettings { get; init; }

    DownloadManagerSettingsModel DownloadManagerSettings { get; init; }

    LanguageSettingsModel LanguageSettings { get; init; }

    ServerSettingsModel ServerSettings { get; init; }

    DebugSettingsModel DebugSettings { get; init; }
}

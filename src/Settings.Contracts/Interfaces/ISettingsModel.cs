namespace Settings.Contracts;

public interface ISettingsModel
{
    GeneralSettingsModule GeneralSettings { get; init; }

    ConfirmationSettingsModule ConfirmationSettings { get; init; }

    DateTimeSettingsModule DateTimeSettings { get; init; }

    DisplaySettingsModule DisplaySettings { get; init; }

    DownloadManagerSettingsModule DownloadManagerSettings { get; init; }

    LanguageSettingsModule LanguageSettings { get; init; }

    PlexServerSettingsModule ServerSettings { get; init; }

    DebugSettingsModule DebugSettings { get; init; }
}

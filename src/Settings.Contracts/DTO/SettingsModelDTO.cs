namespace Settings.Contracts;

public class SettingsModelDTO
{
    public required GeneralSettingsDTO GeneralSettings { get; init; }

    public required DebugSettingsDTO DebugSettings { get; init; }

    public required ConfirmationSettingsDTO ConfirmationSettings { get; init; }

    public required DateTimeSettingsDTO DateTimeSettings { get; init; }

    public required DisplaySettingsDTO DisplaySettings { get; init; }

    public required DownloadManagerSettingsDTO DownloadManagerSettings { get; init; }

    public required LanguageSettingsDTO LanguageSettings { get; init; }

    public required ServerSettingsDTO ServerSettings { get; init; }
}

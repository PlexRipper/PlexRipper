namespace Settings.Contracts;

public class SettingsModelDTO
{
    public required GeneralSettingsDTO GeneralSettings { get; set; }

    public required DebugSettingsDTO DebugSettings { get; set; }

    public required ConfirmationSettingsDTO ConfirmationSettings { get; set; }

    public required DateTimeSettingsDTO DateTimeSettings { get; set; }

    public required DisplaySettingsDTO DisplaySettings { get; set; }

    public required DownloadManagerSettingsDTO DownloadManagerSettings { get; set; }

    public required LanguageSettingsDTO LanguageSettings { get; set; }

    public required ServerSettingsDTO ServerSettings { get; set; }
}
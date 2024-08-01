namespace PlexRipper.Settings;

/// <summary>
/// Used to model the settings, which is then used to serialize to json.
/// </summary>
public record SettingsModel
{
    #region Properties

    public required GeneralSettings GeneralSettings { get; set; }

    public required ConfirmationSettings ConfirmationSettings { get; set; }

    public required DateTimeSettings DateTimeSettings { get; set; }

    public required DisplaySettings DisplaySettings { get; set; }

    public required DownloadManagerSettings DownloadManagerSettings { get; set; }

    public required LanguageSettings LanguageSettings { get; set; }

    public required DebugSettings DebugSettings { get; set; }

    public required ServerSettings ServerSettings { get; set; }

    #endregion
}

namespace PlexRipper.Settings;

/// <summary>
/// Used to model the settings, which is then used to serialize to json.
/// </summary>
public record SettingsModel
{
    #region Properties

    public GeneralSettings GeneralSettings { get; init; } = new();

    public ConfirmationSettings ConfirmationSettings { get; init; } = new();

    public DateTimeSettings DateTimeSettings { get; init; } = new();

    public DisplaySettings DisplaySettings { get; init; } = new();

    public DownloadManagerSettings DownloadManagerSettings { get; init; } = new();

    public LanguageSettings LanguageSettings { get; init; } = new();

    public DebugSettings DebugSettings { get; init; } = new();

    public ServerSettings ServerSettings { get; init; } = new() { Data = [] };

    #endregion
}

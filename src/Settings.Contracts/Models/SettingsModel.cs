namespace Settings.Contracts;

/// <summary>
/// Used to model the settings, which is then used to serialize to json.
/// </summary>
public record SettingsModel
{
    #region Properties

    public GeneralSettingsModel GeneralSettingsModel { get; init; } = new();

    public ConfirmationSettingsModel ConfirmationSettingsModel { get; init; } = new();

    public DateTimeSettingsModel DateTimeSettingsModel { get; init; } = new();

    public DisplaySettingsModel DisplaySettingsModel { get; init; } = new();

    public DownloadManagerSettingsModel DownloadManagerSettingsModel { get; init; } = new();

    public LanguageSettingsModel LanguageSettingsModel { get; init; } = new();

    public DebugSettingsModel DebugSettingsModel { get; init; } = new();

    public ServerSettingsModel ServerSettingsModel { get; init; } = new() { Data = [] };

    #endregion
}

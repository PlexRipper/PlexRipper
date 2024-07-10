using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings;

public static class SettingsModelToDTOMapper
{
    #region ToModel

    public static SettingsModel ToModel(this SettingsModelDTO dto) =>
        new()
        {
            GeneralSettings = dto.GeneralSettings.ToModel(),
            ConfirmationSettings = dto.ConfirmationSettings.ToModel(),
            DateTimeSettings = dto.DateTimeSettings.ToModel(),
            DisplaySettings = dto.DisplaySettings.ToModel(),
            DownloadManagerSettings = dto.DownloadManagerSettings.ToModel(),
            LanguageSettings = dto.LanguageSettings.ToModel(),
            DebugSettings = dto.DebugSettings.ToModel(),
            ServerSettings = dto.ServerSettings.ToModel(),
        };

    public static GeneralSettings ToModel(this GeneralSettingsDTO dto) =>
        new()
        {
            FirstTimeSetup = dto.FirstTimeSetup,
            ActiveAccountId = dto.ActiveAccountId,
            DebugMode = dto.DebugMode,
            DisableAnimatedBackground = dto.DisableAnimatedBackground,
        };

    public static ConfirmationSettings ToModel(this ConfirmationSettingsDTO dto) =>
        new()
        {
            AskDownloadMovieConfirmation = dto.AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = dto.AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = dto.AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = dto.AskDownloadEpisodeConfirmation,
        };

    public static DateTimeSettings ToModel(this DateTimeSettingsDTO dto) =>
        new()
        {
            ShortDateFormat = dto.ShortDateFormat,
            LongDateFormat = dto.LongDateFormat,
            TimeFormat = dto.TimeFormat,
            TimeZone = dto.TimeZone,
            ShowRelativeDates = dto.ShowRelativeDates,
        };

    public static DisplaySettings ToModel(this DisplaySettingsDTO dto) =>
        new() { TvShowViewMode = dto.TvShowViewMode, MovieViewMode = dto.MovieViewMode };

    public static LanguageSettings ToModel(this LanguageSettingsDTO dto) =>
        new() { Language = dto.Language };

    public static DownloadManagerSettings ToModel(this DownloadManagerSettingsDTO dto) =>
        new() { DownloadSegments = dto.DownloadSegments };

    public static ServerSettings ToModel(this ServerSettingsDTO dto) => new() { Data = dto.Data };

    public static DebugSettings ToModel(this DebugSettingsDTO dto) =>
        new()
        {
            DebugModeEnabled = dto.DebugModeEnabled,
            MaskServerNames = dto.MaskServerNames,
            MaskLibraryNames = dto.MaskLibraryNames,
        };

    #endregion

    #region ToDTO

    public static SettingsModelDTO ToDTO(this SettingsModel model) =>
        new()
        {
            GeneralSettings = model.GeneralSettings.ToDTO(),
            DebugSettings = model.DebugSettings.ToDTO(),
            ConfirmationSettings = model.ConfirmationSettings.ToDTO(),
            DateTimeSettings = model.DateTimeSettings.ToDTO(),
            DisplaySettings = model.DisplaySettings.ToDTO(),
            DownloadManagerSettings = model.DownloadManagerSettings.ToDTO(),
            LanguageSettings = model.LanguageSettings.ToDTO(),
            ServerSettings = model.ServerSettings.ToDTO(),
        };

    public static GeneralSettingsDTO ToDTO(this GeneralSettings model) =>
        new()
        {
            FirstTimeSetup = model.FirstTimeSetup,
            ActiveAccountId = model.ActiveAccountId,
            DebugMode = model.DebugMode,
            DisableAnimatedBackground = model.DisableAnimatedBackground,
        };

    public static ConfirmationSettingsDTO ToDTO(this ConfirmationSettings model) =>
        new()
        {
            AskDownloadMovieConfirmation = model.AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = model.AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = model.AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = model.AskDownloadEpisodeConfirmation,
        };

    public static DateTimeSettingsDTO ToDTO(this DateTimeSettings model) =>
        new()
        {
            ShortDateFormat = model.ShortDateFormat,
            LongDateFormat = model.LongDateFormat,
            TimeFormat = model.TimeFormat,
            TimeZone = model.TimeZone,
            ShowRelativeDates = model.ShowRelativeDates,
        };

    public static DisplaySettingsDTO ToDTO(this DisplaySettings model) =>
        new() { TvShowViewMode = model.TvShowViewMode, MovieViewMode = model.MovieViewMode };

    public static LanguageSettingsDTO ToDTO(this LanguageSettings model) =>
        new() { Language = model.Language };

    public static DownloadManagerSettingsDTO ToDTO(this DownloadManagerSettings model) =>
        new() { DownloadSegments = model.DownloadSegments };

    public static ServerSettingsDTO ToDTO(this ServerSettings model) => new() { Data = model.Data };

    public static DebugSettingsDTO ToDTO(this DebugSettings model) =>
        new()
        {
            DebugModeEnabled = model.DebugModeEnabled,
            MaskServerNames = model.MaskServerNames,
            MaskLibraryNames = model.MaskLibraryNames,
        };

    // Interfaces ToDTO

    public static SettingsModelDTO ToDTO(this ISettingsModel model) =>
        (model as SettingsModel).ToDTO();

    public static GeneralSettingsDTO ToDTO(this IGeneralSettings model) =>
        (model as GeneralSettings).ToDTO();

    public static ConfirmationSettingsDTO ToDTO(this IConfirmationSettings model) =>
        (model as ConfirmationSettings).ToDTO();

    public static DateTimeSettingsDTO ToDTO(this IDateTimeSettings model) =>
        (model as DateTimeSettings).ToDTO();

    public static DisplaySettingsDTO ToDTO(this IDisplaySettings model) =>
        (model as DisplaySettings).ToDTO();

    public static LanguageSettingsDTO ToDTO(this ILanguageSettings model) =>
        (model as LanguageSettings).ToDTO();

    public static DownloadManagerSettingsDTO ToDTO(this IDownloadManagerSettings model) =>
        (model as DownloadManagerSettings).ToDTO();

    public static ServerSettingsDTO ToDTO(this IServerSettings model) =>
        (model as ServerSettings).ToDTO();

    public static DebugSettingsDTO ToDTO(this IDebugSettings model) =>
        (model as DebugSettings).ToDTO();

    #endregion
}

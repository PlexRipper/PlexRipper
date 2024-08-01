using Settings.Contracts;

namespace PlexRipper.Settings;

public static class SettingsModelToDTOMapper
{
    #region ToModel

    public static SettingsModel ToModel(this SettingsModelDTO dto) =>
        new()
        {
            GeneralSettingsModel = dto.GeneralSettings.ToModel(),
            ConfirmationSettingsModel = dto.ConfirmationSettings.ToModel(),
            DateTimeSettingsModel = dto.DateTimeSettings.ToModel(),
            DisplaySettingsModel = dto.DisplaySettings.ToModel(),
            DownloadManagerSettingsModel = dto.DownloadManagerSettings.ToModel(),
            LanguageSettingsModel = dto.LanguageSettings.ToModel(),
            DebugSettingsModel = dto.DebugSettings.ToModel(),
            ServerSettingsModel = dto.ServerSettings.ToModel(),
        };

    public static GeneralSettingsModel ToModel(this GeneralSettingsDTO dto) =>
        new()
        {
            FirstTimeSetup = dto.FirstTimeSetup,
            ActiveAccountId = dto.ActiveAccountId,
            DebugMode = dto.DebugMode,
            DisableAnimatedBackground = dto.DisableAnimatedBackground,
        };

    public static ConfirmationSettingsModel ToModel(this ConfirmationSettingsDTO dto) =>
        new()
        {
            AskDownloadMovieConfirmation = dto.AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = dto.AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = dto.AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = dto.AskDownloadEpisodeConfirmation,
        };

    public static DateTimeSettingsModel ToModel(this DateTimeSettingsDTO dto) =>
        new()
        {
            ShortDateFormat = dto.ShortDateFormat,
            LongDateFormat = dto.LongDateFormat,
            TimeFormat = dto.TimeFormat,
            TimeZone = dto.TimeZone,
            ShowRelativeDates = dto.ShowRelativeDates,
        };

    public static DisplaySettingsModel ToModel(this DisplaySettingsDTO dto) =>
        new() { TvShowViewMode = dto.TvShowViewMode, MovieViewMode = dto.MovieViewMode };

    public static LanguageSettingsModel ToModel(this LanguageSettingsDTO dto) => new() { Language = dto.Language };

    public static DownloadManagerSettingsModel ToModel(this DownloadManagerSettingsDTO dto) =>
        new() { DownloadSegments = dto.DownloadSegments };

    public static ServerSettingsModel ToModel(this ServerSettingsDTO dto) => new() { Data = dto.Data };

    public static DebugSettingsModel ToModel(this DebugSettingsDTO dto) =>
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
            GeneralSettings = model.GeneralSettingsModel.ToDTO(),
            DebugSettings = model.DebugSettingsModel.ToDTO(),
            ConfirmationSettings = model.ConfirmationSettingsModel.ToDTO(),
            DateTimeSettings = model.DateTimeSettingsModel.ToDTO(),
            DisplaySettings = model.DisplaySettingsModel.ToDTO(),
            DownloadManagerSettings = model.DownloadManagerSettingsModel.ToDTO(),
            LanguageSettings = model.LanguageSettingsModel.ToDTO(),
            ServerSettings = model.ServerSettingsModel.ToDTO(),
        };

    public static GeneralSettingsDTO ToDTO(this GeneralSettingsModel model) =>
        new()
        {
            FirstTimeSetup = model.FirstTimeSetup,
            ActiveAccountId = model.ActiveAccountId,
            DebugMode = model.DebugMode,
            DisableAnimatedBackground = model.DisableAnimatedBackground,
        };

    public static ConfirmationSettingsDTO ToDTO(this ConfirmationSettingsModel model) =>
        new()
        {
            AskDownloadMovieConfirmation = model.AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = model.AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = model.AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = model.AskDownloadEpisodeConfirmation,
        };

    public static DateTimeSettingsDTO ToDTO(this DateTimeSettingsModel model) =>
        new()
        {
            ShortDateFormat = model.ShortDateFormat,
            LongDateFormat = model.LongDateFormat,
            TimeFormat = model.TimeFormat,
            TimeZone = model.TimeZone,
            ShowRelativeDates = model.ShowRelativeDates,
        };

    public static DisplaySettingsDTO ToDTO(this DisplaySettingsModel model) =>
        new() { TvShowViewMode = model.TvShowViewMode, MovieViewMode = model.MovieViewMode };

    public static LanguageSettingsDTO ToDTO(this LanguageSettingsModel model) => new() { Language = model.Language };

    public static DownloadManagerSettingsDTO ToDTO(this DownloadManagerSettingsModel model) =>
        new() { DownloadSegments = model.DownloadSegments };

    public static ServerSettingsDTO ToDTO(this ServerSettingsModel model) => new() { Data = model.Data };

    public static DebugSettingsDTO ToDTO(this DebugSettingsModel model) =>
        new()
        {
            DebugModeEnabled = model.DebugModeEnabled,
            MaskServerNames = model.MaskServerNames,
            MaskLibraryNames = model.MaskLibraryNames,
        };

    #endregion
}

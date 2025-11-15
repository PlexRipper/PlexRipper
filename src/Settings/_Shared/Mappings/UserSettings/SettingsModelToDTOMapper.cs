using Reaparr.Settings.Contracts;

namespace Reaparr.Settings;

public static class SettingsModelToDTOMapper
{
    #region ToModel

    public static UserSettings ToModel(this SettingsModelDTO dto) =>
        new()
        {
            IntegrationsSettings = dto.IntegrationsSettings.ToModel(),
            GeneralSettings = dto.GeneralSettings.ToModel(),
            ConfirmationSettings = dto.ConfirmationSettings.ToModel(),
            DateTimeSettings = dto.DateTimeSettings.ToModel(),
            DisplaySettings = dto.DisplaySettings.ToModel(),
            DownloadManagerSettings = dto.DownloadManagerSettings.ToModel(),
            LanguageSettings = dto.LanguageSettings.ToModel(),
            DebugSettings = dto.DebugSettings.ToModel(),
            ServerSettings = dto.ServerSettings.ToModel(),
        };

    public static IntegrationsSettings ToModel(this IntegrationsSettingsDTO dto) =>
        new()
        {
            Sonarr = dto.Sonarr.ToModel(),
            Radarr = dto.Radarr.ToModel(),
            ReaparrApiKey = dto.ReaparrApiKey,
            DownloadClientUsername = dto.DownloadClientUsername,
            DownloadClientPassword = dto.DownloadClientPassword,
        };

    public static SonarrSettings ToModel(this SonarrSettingsDTO dto) =>
        new()
        {
            IsConfigured = dto.IsConfigured,
            SonarrBaseUrl = dto.SonarrBaseUrl,
            SonarrApiKey = dto.SonarrApiKey,
        };

    public static RadarrSettings ToModel(this RadarrSettingsDTO dto) =>
        new()
        {
            RadarrBaseUrl = dto.RadarrBaseUrl,
            RadarrApiKey = dto.RadarrApiKey,
            IsConfigured = dto.IsConfigured,
        };

    public static GeneralSettingsModule ToModel(this GeneralSettingsDTO dto) =>
        new()
        {
            FirstTimeSetup = dto.FirstTimeSetup,
            ActiveAccountId = dto.ActiveAccountId,
            DisableAnimatedBackground = dto.DisableAnimatedBackground,
            HideMediaFromOfflineServers = dto.HideMediaFromOfflineServers,
            HideMediaFromOwnedServers = dto.HideMediaFromOwnedServers,
            UseLowQualityPosterImages = dto.UseLowQualityPosterImages,
            HasBeenInvitedToDiscord = dto.HasBeenInvitedToDiscord,
        };

    public static ConfirmationSettingsModule ToModel(this ConfirmationSettingsDTO dto) =>
        new()
        {
            AskDownloadMovieConfirmation = dto.AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = dto.AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = dto.AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = dto.AskDownloadEpisodeConfirmation,
        };

    public static DateTimeSettingsModule ToModel(this DateTimeSettingsDTO dto) =>
        new()
        {
            ShortDateFormat = dto.ShortDateFormat,
            LongDateFormat = dto.LongDateFormat,
            TimeFormat = dto.TimeFormat,
            TimeZone = dto.TimeZone,
            ShowRelativeDates = dto.ShowRelativeDates,
        };

    public static DisplaySettingsModule ToModel(this DisplaySettingsDTO dto) =>
        new()
        {
            TvShowViewMode = dto.TvShowViewMode,
            MovieViewMode = dto.MovieViewMode,
            AllOverviewViewMode = dto.AllOverviewViewMode,
        };

    public static LanguageSettingsModule ToModel(this LanguageSettingsDTO dto) => new() { Language = dto.Language };

    public static DownloadManagerSettingsModule ToModel(this DownloadManagerSettingsDTO dto) =>
        new()
        {
            DownloadSegments = dto.DownloadSegments,
            KeepCompletedInDownloadFolder = dto.KeepCompletedInDownloadFolder,
        };

    public static PlexServerSettingsModule ToModel(this ServerSettingsDTO dto) => new() { Data = dto.Data };

    public static DebugSettingsModule ToModel(this DebugSettingsDTO dto) =>
        new()
        {
            DebugModeEnabled = dto.DebugModeEnabled,
            MaskServerNames = dto.MaskServerNames,
            MaskLibraryNames = dto.MaskLibraryNames,
        };

    #endregion

    #region ToDTO

    public static SettingsModelDTO ToDTO(this IUserSettings model) =>
        new()
        {
            IntegrationsSettings = model.IntegrationsSettings.ToDTO(),
            GeneralSettings = model.GeneralSettings.ToDTO(),
            DebugSettings = model.DebugSettings.ToDTO(),
            ConfirmationSettings = model.ConfirmationSettings.ToDTO(),
            DateTimeSettings = model.DateTimeSettings.ToDTO(),
            DisplaySettings = model.DisplaySettings.ToDTO(),
            DownloadManagerSettings = model.DownloadManagerSettings.ToDTO(),
            LanguageSettings = model.LanguageSettings.ToDTO(),
            ServerSettings = model.ServerSettings.ToDTO(),
        };

    public static IntegrationsSettingsDTO ToDTO(this IntegrationsSettings module) =>
        new()
        {
            ReaparrApiKey = module.ReaparrApiKey,
            Sonarr = module.Sonarr.ToDTO(),
            Radarr = module.Radarr.ToDTO(),
            DownloadClientUsername = module.DownloadClientUsername,
            DownloadClientPassword = module.DownloadClientPassword,
        };

    public static SonarrSettingsDTO ToDTO(this SonarrSettings module) =>
        new()
        {
            SonarrBaseUrl = module.SonarrBaseUrl,
            SonarrApiKey = module.SonarrApiKey,
            IsConfigured = module.IsConfigured,
        };

    public static RadarrSettingsDTO ToDTO(this RadarrSettings module) =>
        new()
        {
            RadarrBaseUrl = module.RadarrBaseUrl,
            RadarrApiKey = module.RadarrApiKey,
            IsConfigured = module.IsConfigured,
        };

    public static GeneralSettingsDTO ToDTO(this GeneralSettingsModule module) =>
        new()
        {
            FirstTimeSetup = module.FirstTimeSetup,
            ActiveAccountId = module.ActiveAccountId,
            DisableAnimatedBackground = module.DisableAnimatedBackground,
            HideMediaFromOfflineServers = module.HideMediaFromOfflineServers,
            HideMediaFromOwnedServers = module.HideMediaFromOwnedServers,
            UseLowQualityPosterImages = module.UseLowQualityPosterImages,
            HasBeenInvitedToDiscord = module.HasBeenInvitedToDiscord,
            HasAgreedToDisclaimer = module.HasAgreedToDisclaimer,
        };

    public static ConfirmationSettingsDTO ToDTO(this ConfirmationSettingsModule module) =>
        new()
        {
            AskDownloadMovieConfirmation = module.AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = module.AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = module.AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = module.AskDownloadEpisodeConfirmation,
        };

    public static DateTimeSettingsDTO ToDTO(this DateTimeSettingsModule module) =>
        new()
        {
            ShortDateFormat = module.ShortDateFormat,
            LongDateFormat = module.LongDateFormat,
            TimeFormat = module.TimeFormat,
            ShowRelativeDates = module.ShowRelativeDates,
            TimeZone = module.TimeZone,
        };

    public static DisplaySettingsDTO ToDTO(this DisplaySettingsModule module) =>
        new()
        {
            TvShowViewMode = module.TvShowViewMode,
            MovieViewMode = module.MovieViewMode,
            AllOverviewViewMode = module.AllOverviewViewMode,
        };

    public static LanguageSettingsDTO ToDTO(this LanguageSettingsModule module) => new() { Language = module.Language };

    public static DownloadManagerSettingsDTO ToDTO(this DownloadManagerSettingsModule module) =>
        new()
        {
            DownloadSegments = module.DownloadSegments,
            KeepCompletedInDownloadFolder = module.KeepCompletedInDownloadFolder,
        };

    public static ServerSettingsDTO ToDTO(this PlexServerSettingsModule module) => new() { Data = module.Data };

    public static DebugSettingsDTO ToDTO(this DebugSettingsModule module) =>
        new()
        {
            DebugModeEnabled = module.DebugModeEnabled,
            MaskServerNames = module.MaskServerNames,
            MaskLibraryNames = module.MaskLibraryNames,
        };

    #endregion
}

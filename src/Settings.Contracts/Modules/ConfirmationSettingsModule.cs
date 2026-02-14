namespace Reaparr.Settings.Contracts;

public record ConfirmationSettingsModule
    : BaseSettingsModule<ConfirmationSettingsModule>,
        IBaseSettingsModule<ConfirmationSettingsModule>,
        IConfirmationSettings
{
    public static ConfirmationSettingsModule Create() =>
        new()
        {
            AskDownloadMovieConfirmation = true,
            AskDownloadTvShowConfirmation = true,
            AskDownloadSeasonConfirmation = true,
            AskDownloadEpisodeConfirmation = true,
        };

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading a movie.
    /// </summary>
    public required bool AskDownloadMovieConfirmation
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading a TV show.
    /// </summary>
    public required bool AskDownloadTvShowConfirmation
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading a season.
    /// </summary>
    public required bool AskDownloadSeasonConfirmation
    {
        get;
        set => SetProperty(ref field, value);
    } = true;

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading an episode.
    /// </summary>
    public required bool AskDownloadEpisodeConfirmation
    {
        get;
        set => SetProperty(ref field, value);
    } = true;
}

namespace Reaparr.Settings.Contracts;

public record ConfirmationSettingsModule : BaseSettingsModule<ConfirmationSettingsModule>, IConfirmationSettings
{
    private bool _askDownloadMovieConfirmation = true;
    private bool _askDownloadTvShowConfirmation = true;
    private bool _askDownloadSeasonConfirmation = true;
    private bool _askDownloadEpisodeConfirmation = true;

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
        get => _askDownloadMovieConfirmation;
        set => SetProperty(ref _askDownloadMovieConfirmation, value);
    }

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading a TV show.
    /// </summary>
    public required bool AskDownloadTvShowConfirmation
    {
        get => _askDownloadTvShowConfirmation;
        set => SetProperty(ref _askDownloadTvShowConfirmation, value);
    }

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading a season.
    /// </summary>
    public required bool AskDownloadSeasonConfirmation
    {
        get => _askDownloadSeasonConfirmation;
        set => SetProperty(ref _askDownloadSeasonConfirmation, value);
    }

    /// <summary>
    /// Indicates whether to ask for confirmation before downloading an episode.
    /// </summary>
    public required bool AskDownloadEpisodeConfirmation
    {
        get => _askDownloadEpisodeConfirmation;
        set => SetProperty(ref _askDownloadEpisodeConfirmation, value);
    }
}

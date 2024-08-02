namespace Settings.Contracts;

public record ConfirmationSettingsModule : BaseSettingsModule<ConfirmationSettingsModule>, IConfirmationSettings
{
    private bool _askDownloadMovieConfirmation = true;
    private bool _askDownloadTvShowConfirmation = true;
    private bool _askDownloadSeasonConfirmation = true;
    private bool _askDownloadEpisodeConfirmation = true;

    public bool AskDownloadMovieConfirmation
    {
        get => _askDownloadMovieConfirmation;
        set => SetProperty(ref _askDownloadMovieConfirmation, value);
    }

    public bool AskDownloadTvShowConfirmation
    {
        get => _askDownloadTvShowConfirmation;
        set => SetProperty(ref _askDownloadTvShowConfirmation, value);
    }

    public bool AskDownloadSeasonConfirmation
    {
        get => _askDownloadSeasonConfirmation;
        set => SetProperty(ref _askDownloadSeasonConfirmation, value);
    }

    public bool AskDownloadEpisodeConfirmation
    {
        get => _askDownloadEpisodeConfirmation;
        set => SetProperty(ref _askDownloadEpisodeConfirmation, value);
    }
}

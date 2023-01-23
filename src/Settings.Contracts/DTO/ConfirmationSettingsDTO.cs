namespace Settings.Contracts;

public class ConfirmationSettingsDTO : IConfirmationSettings
{
    public bool AskDownloadMovieConfirmation { get; set; }

    public bool AskDownloadTvShowConfirmation { get; set; }

    public bool AskDownloadSeasonConfirmation { get; set; }

    public bool AskDownloadEpisodeConfirmation { get; set; }
}
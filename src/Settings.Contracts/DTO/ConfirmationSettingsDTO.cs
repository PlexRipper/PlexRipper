namespace Settings.Contracts;

public class ConfirmationSettingsDTO : IConfirmationSettings
{
    public required bool AskDownloadMovieConfirmation { get; set; }

    public required bool AskDownloadTvShowConfirmation { get; set; }

    public required bool AskDownloadSeasonConfirmation { get; set; }

    public required bool AskDownloadEpisodeConfirmation { get; set; }
}

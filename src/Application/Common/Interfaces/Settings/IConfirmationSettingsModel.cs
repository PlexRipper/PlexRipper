namespace PlexRipper.Application.Common
{
    public interface IConfirmationSettingsModel
    {
        bool AskDownloadMovieConfirmation { get; set; }

        bool AskDownloadTvShowConfirmation { get; set; }

        bool AskDownloadSeasonConfirmation { get; set; }

        bool AskDownloadEpisodeConfirmation { get; set; }
    }
}
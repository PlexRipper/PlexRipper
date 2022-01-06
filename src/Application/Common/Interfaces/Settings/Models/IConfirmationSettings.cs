namespace PlexRipper.Application
{
    public interface IConfirmationSettings
    {
        bool AskDownloadMovieConfirmation { get; set; }

        bool AskDownloadTvShowConfirmation { get; set; }

        bool AskDownloadSeasonConfirmation { get; set; }

        bool AskDownloadEpisodeConfirmation { get; set; }
    }
}
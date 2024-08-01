using Settings.Contracts;

namespace PlexRipper.Settings;

public record ConfirmationSettings : IConfirmationSettings
{
    public bool AskDownloadMovieConfirmation { get; set; } = true;

    public bool AskDownloadTvShowConfirmation { get; set; } = true;

    public bool AskDownloadSeasonConfirmation { get; set; } = true;

    public bool AskDownloadEpisodeConfirmation { get; set; } = true;
}

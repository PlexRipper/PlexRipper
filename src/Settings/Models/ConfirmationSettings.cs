using PlexRipper.Application;
using Settings.Contracts;

namespace PlexRipper.Settings.Models;

public class ConfirmationSettings : IConfirmationSettings
{
    public bool AskDownloadMovieConfirmation { get; set; }

    public bool AskDownloadTvShowConfirmation { get; set; }

    public bool AskDownloadSeasonConfirmation { get; set; }

    public bool AskDownloadEpisodeConfirmation { get; set; }
}
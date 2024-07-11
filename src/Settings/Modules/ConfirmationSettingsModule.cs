using PlexRipper.Settings.Models;
using Settings.Contracts;

namespace PlexRipper.Settings.Modules;

public sealed class ConfirmationSettingsModule : BaseSettingsModule<IConfirmationSettings>, IConfirmationSettingsModule
{
    #region Properties

    public override string Name => "ConfirmationSettings";

    public override IConfirmationSettings DefaultValues()
    {
        return new ConfirmationSettings
        {
            AskDownloadEpisodeConfirmation = true,
            AskDownloadMovieConfirmation = true,
            AskDownloadSeasonConfirmation = true,
            AskDownloadTvShowConfirmation = true,
        };
    }

    public bool AskDownloadMovieConfirmation { get; set; }

    public bool AskDownloadTvShowConfirmation { get; set; }

    public bool AskDownloadSeasonConfirmation { get; set; }

    public bool AskDownloadEpisodeConfirmation { get; set; }

    #endregion

    #region Public Methods

    public override IConfirmationSettings GetValues()
    {
        return new ConfirmationSettings
        {
            AskDownloadMovieConfirmation = AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = AskDownloadEpisodeConfirmation,
        };
    }

    #endregion
}

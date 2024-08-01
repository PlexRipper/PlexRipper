using Settings.Contracts;

namespace PlexRipper.Settings;

public sealed class ConfirmationSettingsModule : BaseSettingsModule<IConfirmationSettings>, IConfirmationSettingsModule
{
    #region Properties

    public override string Name => "ConfirmationSettings";

    public override IConfirmationSettings DefaultValues() =>
        new ConfirmationSettings
        {
            AskDownloadEpisodeConfirmation = true,
            AskDownloadMovieConfirmation = true,
            AskDownloadSeasonConfirmation = true,
            AskDownloadTvShowConfirmation = true,
        };

    public bool AskDownloadMovieConfirmation { get; set; }

    public bool AskDownloadTvShowConfirmation { get; set; }

    public bool AskDownloadSeasonConfirmation { get; set; }

    public bool AskDownloadEpisodeConfirmation { get; set; }

    #endregion

    #region Public Methods

    public override IConfirmationSettings GetValues() =>
        new ConfirmationSettings
        {
            AskDownloadMovieConfirmation = AskDownloadMovieConfirmation,
            AskDownloadTvShowConfirmation = AskDownloadTvShowConfirmation,
            AskDownloadSeasonConfirmation = AskDownloadSeasonConfirmation,
            AskDownloadEpisodeConfirmation = AskDownloadEpisodeConfirmation,
        };

    #endregion
}

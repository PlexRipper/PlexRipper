using PlexRipper.Domain;

namespace PlexRipper.Application.Common
{
    public interface ISettingsModel
    {
        #region Main

        public bool FirstTimeSetup { get; set; }

        #endregion

        #region AccountSettings

        public int ActiveAccountId { get; set; }

        #endregion

        #region AdvancedSettings

        #region DownloadManagerSettings

        public int DownloadSegments { get; set; }

        #endregion

        #endregion

        #region UserInterfaceSettings

        #region ConfirmationSettings

        public bool AskDownloadMovieConfirmation { get; set; }

        public bool AskDownloadTvShowConfirmation { get; set; }

        public bool AskDownloadSeasonConfirmation { get; set; }

        public bool AskDownloadEpisodeConfirmation { get; set; }

        #endregion

        #region DisplaySettingsModel

        public ViewMode TvShowViewMode { get; set; }

        public ViewMode MovieViewMode { get; set; }

        #endregion

        #region DateTimeModel

        public string ShortDateFormat { get; set; }

        public string LongDateFormat { get; set; }

        public string TimeFormat { get; set; }

        public string TimeZone { get; set; }

        public bool ShowRelativeDates { get; set; }

        #endregion

        #endregion
    }
}
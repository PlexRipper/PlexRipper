using System.Dynamic;
using System.Text.Json;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.Settings.Models
{
    /// <summary>
    /// Used to model the settings, which is then used to serialize to json.
    /// </summary>
    public class SettingsModel : ISettingsModel
    {
        #region Properties

        #region Main

        public bool FirstTimeSetup { get; set; } = true;

        #endregion

        #region AccountSettings

        public int ActiveAccountId { get; set; }

        #endregion

        #region AdvancedSettings

        #region DownloadManagerSettings

        public int DownloadSegments { get; set; } = 4;

        #endregion

        #endregion

        #region UserInterfaceSettings

        #region ConfirmationSettings

        public bool AskDownloadMovieConfirmation { get; set; } = true;

        public bool AskDownloadTvShowConfirmation { get; set; } = true;

        public bool AskDownloadSeasonConfirmation { get; set; } = true;

        public bool AskDownloadEpisodeConfirmation { get; set; } = true;

        #endregion

        #region DisplaySettingsModel

        public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        #endregion

        #region DateTimeModel

        public string ShortDateFormat { get; set; } = "dd/MM/yyyy";

        public string LongDateFormat { get; set; } = "EEEE, dd MMMM yyyy";

        public string TimeFormat { get; set; } = "HH:MM:ss";

        public string TimeZone { get; set; } = "UTC";

        public bool ShowRelativeDates { get; set; } = true;

        #endregion

        #endregion

        #endregion Properties

        #region Methods

        protected dynamic GetJsonObject()
        {
            dynamic jsonObject = new ExpandoObject();
            jsonObject.FirstTimeSetup = FirstTimeSetup;

            // Account Settings
            jsonObject.AccountSettings = new ExpandoObject();
            jsonObject.AccountSettings.ActiveAccountId = ActiveAccountId;

            // Advanced Settings
            jsonObject.AdvancedSettings = new ExpandoObject();
            jsonObject.AdvancedSettings.DownloadManagerSettings = new ExpandoObject();
            jsonObject.AdvancedSettings.DownloadManagerSettings.DownloadSegments = DownloadSegments;

            // User Interface Settings
            jsonObject.UserInterfaceSettings = new ExpandoObject();
            jsonObject.UserInterfaceSettings.ConfirmationSettings = new ExpandoObject();
            jsonObject.UserInterfaceSettings.ConfirmationSettings.AskDownloadMovieConfirmation = AskDownloadMovieConfirmation;
            jsonObject.UserInterfaceSettings.ConfirmationSettings.AskDownloadTvShowConfirmation = AskDownloadTvShowConfirmation;
            jsonObject.UserInterfaceSettings.ConfirmationSettings.AskDownloadSeasonConfirmation = AskDownloadSeasonConfirmation;
            jsonObject.UserInterfaceSettings.ConfirmationSettings.AskDownloadEpisodeConfirmation = AskDownloadEpisodeConfirmation;

            jsonObject.UserInterfaceSettings.DisplaySettings = new ExpandoObject();
            jsonObject.UserInterfaceSettings.DisplaySettings.TvShowViewMode = TvShowViewMode.ToString();
            jsonObject.UserInterfaceSettings.DisplaySettings.MovieViewMode = MovieViewMode.ToString();

            jsonObject.UserInterfaceSettings.DateTimeSettings = new ExpandoObject();
            jsonObject.UserInterfaceSettings.DateTimeSettings.ShortDateFormat = ShortDateFormat;
            jsonObject.UserInterfaceSettings.DateTimeSettings.LongDateFormat = LongDateFormat;
            jsonObject.UserInterfaceSettings.DateTimeSettings.TimeFormat = TimeFormat;
            jsonObject.UserInterfaceSettings.DateTimeSettings.TimeZone = TimeZone;
            jsonObject.UserInterfaceSettings.DateTimeSettings.ShowRelativeDates = ShowRelativeDates;

            return jsonObject;
        }

        protected void SetFromJsonObject(JsonElement jsonElement)
        {
            FirstTimeSetup = jsonElement.GetProperty(nameof(FirstTimeSetup)).GetBoolean();

            // Account Settings
            ActiveAccountId = jsonElement.GetProperty("AccountSettings").GetProperty(nameof(ActiveAccountId)).GetInt32();

            // Advanced Settings
            var advancedSettings = jsonElement.GetProperty("AdvancedSettings");
            DownloadSegments = advancedSettings.GetProperty("DownloadManagerSettings").GetProperty(nameof(DownloadSegments)).GetInt32();

            var userInterfaceSettings = jsonElement.GetProperty("UserInterfaceSettings");

            var confirmationSettings = userInterfaceSettings.GetProperty("ConfirmationSettings");
            AskDownloadMovieConfirmation = confirmationSettings.GetProperty(nameof(AskDownloadMovieConfirmation)).GetBoolean();
            AskDownloadTvShowConfirmation = confirmationSettings.GetProperty(nameof(AskDownloadTvShowConfirmation)).GetBoolean();
            AskDownloadSeasonConfirmation = confirmationSettings.GetProperty(nameof(AskDownloadSeasonConfirmation)).GetBoolean();
            AskDownloadEpisodeConfirmation = confirmationSettings.GetProperty(nameof(AskDownloadEpisodeConfirmation)).GetBoolean();

            var displaySettings = userInterfaceSettings.GetProperty("DisplaySettings");
            MovieViewMode = displaySettings.GetProperty(nameof(MovieViewMode)).ToString().ToViewMode();
            TvShowViewMode = displaySettings.GetProperty(nameof(TvShowViewMode)).ToString().ToViewMode();

            var dateTimeSettings = userInterfaceSettings.GetProperty("DateTimeSettings");
            TimeFormat = dateTimeSettings.GetProperty(nameof(TimeFormat)).ToString();
            TimeZone = dateTimeSettings.GetProperty(nameof(TimeZone)).ToString();
            ShortDateFormat = dateTimeSettings.GetProperty(nameof(ShortDateFormat)).ToString();
            LongDateFormat = dateTimeSettings.GetProperty(nameof(LongDateFormat)).ToString();
            ShowRelativeDates = dateTimeSettings.GetProperty(nameof(ShowRelativeDates)).GetBoolean();
        }

        #endregion
    }
}
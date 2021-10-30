using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using FluentResults;
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

        public Dictionary<string, int> DownloadLimit { get; set; }

        #endregion

        #endregion

        #region UserInterfaceSettings

        #region Language

        public string Language { get; set; } = "en-US";

        #endregion

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

        public string TimeFormat { get; set; } = "HH:mm:ss";

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
            jsonObject.AdvancedSettings.DownloadManagerSettings.DownloadLimit = DownloadLimit;

            // User Interface Settings
            jsonObject.UserInterfaceSettings = new ExpandoObject();
            jsonObject.UserInterfaceSettings.Language = Language;
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

        private bool TryGetBoolean(JsonElement jsonElement, string property, bool defaultValue, Action<Result> failed)
        {
            try
            {
                if (jsonElement.TryGetProperty(property, out jsonElement))
                {
                    failed(Result.Ok());
                    return jsonElement.GetBoolean();
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            failed(Result.Fail($"Failed to retrieve boolean property: {property}"));
            return defaultValue;
        }

        private int TryGetInteger(JsonElement jsonElement, string property, int defaultValue, Action<Result> failed)
        {
            try
            {
                if (jsonElement.TryGetProperty(property, out jsonElement))
                {
                    if (jsonElement.TryGetInt32(out int value))
                    {
                        failed(Result.Ok());
                        return value;
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            failed(Result.Fail($"Failed to retrieve integer property: {property}"));
            return defaultValue;
        }

        private string TryGetString(JsonElement jsonElement, string property, string defaultValue, Action<Result> failed)
        {
            try
            {
                if (jsonElement.TryGetProperty(property, out jsonElement))
                {
                    var value = jsonElement.GetString();
                    if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
                    {
                        failed(Result.Ok());
                        return value;
                    }
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            failed(Result.Fail($"Failed to retrieve string property: {property}"));
            return defaultValue;
        }

        /// <summary>
        /// Parses the Json Element from the PlexRipperSettings.json and defaults its value if nothing is found.
        /// This also works when adding new settings and ensuring old config files get used as much as possible.
        /// </summary>
        /// <param name="settingsJsonElement"></param>
        protected Result SetFromJsonObject(JsonElement settingsJsonElement)
        {
            List<Result> results = new List<Result>();

            var addResult = new Action<Result>(result => results.Add(result));

            FirstTimeSetup = TryGetBoolean(settingsJsonElement, nameof(FirstTimeSetup), FirstTimeSetup, addResult);

            // Account Settings
            if (settingsJsonElement.TryGetProperty("AccountSettings", out JsonElement accountSettings))
            {
                // ActiveAccountId
                ActiveAccountId = TryGetInteger(accountSettings, nameof(ActiveAccountId), ActiveAccountId, addResult);
            }
            else
            {
                addResult(Result.Fail("Failed to retrieve property AccountSettings"));
            }

            // Advanced Settings
            if (settingsJsonElement.TryGetProperty("AdvancedSettings", out JsonElement advancedSettings))
            {
                // DownloadManagerSettings
                if (advancedSettings.TryGetProperty("DownloadManagerSettings", out JsonElement downloadManagerSettings))
                {
                    // DownloadSegments
                    DownloadSegments = TryGetInteger(downloadManagerSettings, nameof(DownloadSegments), DownloadSegments, addResult);

                    if (downloadManagerSettings.TryGetProperty(nameof(DownloadLimit), out var downloadLimit))
                    {
                        DownloadLimit = JsonSerializer.Deserialize<Dictionary<string, int>>(downloadLimit.GetRawText());
                    }
                    else
                    {
                        addResult(Result.Fail("Failed to retrieve property DownloadManagerSettings.DownloadLimit"));
                    }
                }
                else
                {
                    addResult(Result.Fail("Failed to retrieve property DownloadManagerSettings"));
                }
            }
            else
            {
                addResult(Result.Fail("Failed to retrieve property AdvancedSettings"));
            }

            // User Interface Settings
            if (settingsJsonElement.TryGetProperty("UserInterfaceSettings", out JsonElement userInterfaceSettings))
            {
                Language = TryGetString(userInterfaceSettings, nameof(Language), Language, addResult);

                // Confirmation Settings
                if (userInterfaceSettings.TryGetProperty("ConfirmationSettings", out JsonElement confirmationSettings))
                {
                    // Ask Confirmation Settings on Media Type
                    AskDownloadMovieConfirmation = TryGetBoolean(confirmationSettings, nameof(AskDownloadMovieConfirmation),
                        AskDownloadMovieConfirmation, addResult);

                    AskDownloadTvShowConfirmation = TryGetBoolean(confirmationSettings, nameof(AskDownloadTvShowConfirmation),
                        AskDownloadTvShowConfirmation, addResult);

                    AskDownloadSeasonConfirmation = TryGetBoolean(confirmationSettings, nameof(AskDownloadSeasonConfirmation),
                        AskDownloadSeasonConfirmation, addResult);

                    AskDownloadEpisodeConfirmation = TryGetBoolean(confirmationSettings, nameof(AskDownloadEpisodeConfirmation),
                        AskDownloadEpisodeConfirmation, addResult);
                }
                else
                {
                    addResult(Result.Fail("Failed to retrieve property ConfirmationSettings"));
                }

                // Display Settings
                if (userInterfaceSettings.TryGetProperty("DisplaySettings", out JsonElement displaySettings))
                {
                    MovieViewMode = TryGetString(displaySettings, nameof(MovieViewMode), MovieViewMode.ToViewModeString(), addResult).ToViewMode();
                    TvShowViewMode = TryGetString(displaySettings, nameof(TvShowViewMode), TvShowViewMode.ToViewModeString(), addResult).ToViewMode();
                }
                else
                {
                    addResult(Result.Fail("Failed to retrieve property DisplaySettings"));
                }

                // DateTime Settings
                if (userInterfaceSettings.TryGetProperty("DateTimeSettings", out JsonElement dateTimeSettings))
                {
                    TimeFormat = TryGetString(dateTimeSettings, nameof(TimeFormat), TimeFormat, addResult);
                    TimeZone = TryGetString(dateTimeSettings, nameof(TimeZone), TimeZone, addResult);
                    ShortDateFormat = TryGetString(dateTimeSettings, nameof(ShortDateFormat), ShortDateFormat, addResult);
                    LongDateFormat = TryGetString(dateTimeSettings, nameof(LongDateFormat), LongDateFormat, addResult);
                    ShowRelativeDates = TryGetBoolean(dateTimeSettings, nameof(ShowRelativeDates), ShowRelativeDates, addResult);
                }
                else
                {
                    addResult(Result.Fail("Failed to retrieve property DateTimeSettings"));
                }
            }
            else
            {
                addResult(Result.Fail("Failed to retrieve property UserInterfaceSettings"));
            }

            // Check if there are any failures
            if (results.Select(x => x.IsFailed).Any(x => x))
            {
                return Result.Fail("Parsing the PlexRipperSettings.json had errors:").WithErrors(results.SelectMany(x => x.Errors));
            }

            return Result.Ok();
        }

        #endregion
    }
}
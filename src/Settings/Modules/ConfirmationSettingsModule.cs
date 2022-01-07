using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class ConfirmationSettingsModule : BaseSettingsModule<IConfirmationSettings>, IConfirmationSettingsModule
    {
        #region Properties

        public override string Name => "ConfirmationSettings";

        public bool AskDownloadEpisodeConfirmation { get; set; } = true;

        public bool AskDownloadMovieConfirmation { get; set; } = true;

        public bool AskDownloadSeasonConfirmation { get; set; } = true;

        public bool AskDownloadTvShowConfirmation { get; set; } = true;

        #endregion

        #region Public Methods

        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var confirmationSettings = jsonSettings.Value;

            if (confirmationSettings.TryGetProperty(nameof(AskDownloadMovieConfirmation), out JsonElement askDownloadMovieConfirmation))
            {
                AskDownloadMovieConfirmation = askDownloadMovieConfirmation.GetBoolean();
            }

            if (confirmationSettings.TryGetProperty(nameof(AskDownloadTvShowConfirmation), out JsonElement askDownloadTvShowConfirmation))
            {
                AskDownloadTvShowConfirmation = askDownloadTvShowConfirmation.GetBoolean();
            }

            if (confirmationSettings.TryGetProperty(nameof(AskDownloadSeasonConfirmation), out JsonElement askDownloadSeasonConfirmation))
            {
                AskDownloadSeasonConfirmation = askDownloadSeasonConfirmation.GetBoolean();
            }

            if (confirmationSettings.TryGetProperty(nameof(AskDownloadEpisodeConfirmation), out JsonElement askDownloadEpisodeConfirmation))
            {
                AskDownloadEpisodeConfirmation = askDownloadEpisodeConfirmation.GetBoolean();
            }

            return Result.Ok();
        }

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
}
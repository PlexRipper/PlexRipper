using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class ConfirmationSettingsModule : BaseSettingsModule<IConfirmationSettings>, IConfirmationSettingsModule
    {
        #region Properties

        public bool AskDownloadEpisodeConfirmation { get; set; } = true;

        public bool AskDownloadMovieConfirmation { get; set; } = true;

        public bool AskDownloadSeasonConfirmation { get; set; } = true;

        public bool AskDownloadTvShowConfirmation { get; set; } = true;

        public override string Name => "ConfirmationSettings";

        #endregion

        #region Public Methods

        public void Reset()
        {
            Update(new ConfirmationSettings());
        }

        public Result SetFromJsonObject(JsonElement settingsJsonElement)
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

        public IConfirmationSettings GetValues()
        {
            return new ConfirmationSettings
            {
                AskDownloadMovieConfirmation = AskDownloadMovieConfirmation,
                AskDownloadTvShowConfirmation = AskDownloadTvShowConfirmation,
                AskDownloadSeasonConfirmation = AskDownloadSeasonConfirmation,
                AskDownloadEpisodeConfirmation = AskDownloadEpisodeConfirmation,
            };
        }

        public Result Update(IConfirmationSettings sourceSettings)
        {
            var hasChanged = false;
            if (AskDownloadMovieConfirmation != sourceSettings.AskDownloadMovieConfirmation)
            {
                AskDownloadMovieConfirmation = sourceSettings.AskDownloadMovieConfirmation;
                hasChanged = true;
            }

            if (AskDownloadMovieConfirmation != sourceSettings.AskDownloadMovieConfirmation)
            {
                AskDownloadTvShowConfirmation = sourceSettings.AskDownloadTvShowConfirmation;
                hasChanged = true;
            }

            if (AskDownloadMovieConfirmation != sourceSettings.AskDownloadMovieConfirmation)
            {
                AskDownloadSeasonConfirmation = sourceSettings.AskDownloadSeasonConfirmation;
                hasChanged = true;
            }

            if (AskDownloadMovieConfirmation != sourceSettings.AskDownloadMovieConfirmation)
            {
                AskDownloadEpisodeConfirmation = sourceSettings.AskDownloadEpisodeConfirmation;
                hasChanged = true;
            }

            if (hasChanged)
            {
                EmitModuleHasChanged(GetValues());
            }

            return Result.Ok();
        }

        #endregion
    }
}
using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class DisplaySettingsModule : BaseSettingsModule<IDisplaySettings>, IDisplaySettingsModule
    {
        public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        public override string Name => "DisplaySettings";

        public void Reset()
        {
            Update(new DisplaySettingsModule());
        }

        public Result SetFromJson(JsonElement settingsJsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(settingsJsonElement);
            if (jsonSettings.IsFailed)
            {
                Reset();
                return jsonSettings;
            }

            var displaySettings = jsonSettings.Value;

            if (displaySettings.TryGetProperty(nameof(TvShowViewMode), out JsonElement tvShowViewMode))
            {
                TvShowViewMode = tvShowViewMode.GetString().ToViewMode();
            }

            if (displaySettings.TryGetProperty(nameof(MovieViewMode), out JsonElement movieViewMode))
            {
                MovieViewMode = movieViewMode.GetString().ToViewMode();
            }

            return Result.Ok();
        }

        public override IDisplaySettings GetValues()
        {
            return new DisplaySettings
            {
                MovieViewMode = MovieViewMode,
                TvShowViewMode = TvShowViewMode,
            };
        }
    }
}
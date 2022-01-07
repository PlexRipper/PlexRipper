using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;

namespace PlexRipper.Settings.Modules
{
    public class DisplaySettingsModule : BaseSettingsModule<IDisplaySettings>, IDisplaySettingsModule
    {
        public ViewMode TvShowViewMode { get; set; }

        public ViewMode MovieViewMode { get; set; }

        public override string Name => "DisplaySettings";

        public override IDisplaySettings DefaultValues => new DisplaySettings
        {
            TvShowViewMode = ViewMode.Poster,
            MovieViewMode = ViewMode.Poster,
        };

        public override Result SetFromJson(JsonElement jsonElement)
        {
            var jsonSettings = GetJsonSettingsModule(jsonElement);
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
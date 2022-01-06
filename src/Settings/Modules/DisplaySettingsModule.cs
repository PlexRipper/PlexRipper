using System;
using System.Text.Json;
using FluentResults;
using PlexRipper.Application;
using PlexRipper.Domain;

namespace PlexRipper.Settings.Modules
{
    public class DisplaySettingsModule :  IDisplaySettingsModule
    {
        public ViewMode TvShowViewMode { get; set; } = ViewMode.Poster;

        public ViewMode MovieViewMode { get; set; } = ViewMode.Poster;

        public string Name => "DisplaySettings";

        public Result Update(IDisplaySettings sourceSettings)
        {
            TvShowViewMode = sourceSettings.TvShowViewMode;
            MovieViewMode = sourceSettings.MovieViewMode;
            return Result.Ok();
        }

        public void Reset()
        {
            Update(new DisplaySettingsModule());
        }

        public Result SetFromJsonObject(JsonElement settingsJsonElement)
        {
            throw new System.NotImplementedException();
        }

        public IObservable<IDisplaySettings> ModuleHasChanged { get; }

        public IDisplaySettings GetValues()
        {
            throw new NotImplementedException();
        }
    }
}
﻿using System.Text.Json;
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

        public override IDisplaySettings DefaultValues()
        {
            return new DisplaySettings
            {
                TvShowViewMode = ViewMode.Poster,
                MovieViewMode = ViewMode.Poster,
            };
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
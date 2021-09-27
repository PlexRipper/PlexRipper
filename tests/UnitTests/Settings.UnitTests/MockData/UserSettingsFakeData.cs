using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Bogus;
using Bogus.Platform;
using PlexRipper.BaseTests;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;

namespace Settings.UnitTests.MockData
{
    public static class UserSettingsFakeData
    {
        private static readonly Random _random = new();

        public static Faker<SettingsModel> GetSettingsModel()
        {
            return new Faker<SettingsModel>()
                .UseSeed(_random.Next(1, 100))
                .RuleFor(x => x.FirstTimeSetup, f => f.Random.Bool())
                .RuleFor(x => x.ActiveAccountId, f => f.Random.Int(1, 99))
                .RuleFor(x => x.DownloadSegments, f => f.Random.Int(1, 99))
                .RuleFor(x => x.AskDownloadMovieConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.AskDownloadTvShowConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.AskDownloadSeasonConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.AskDownloadEpisodeConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.TvShowViewMode, f => f.Random.Enum<ViewMode>())
                .RuleFor(x => x.MovieViewMode, f => f.Random.Enum<ViewMode>())
                .RuleFor(x => x.ShortDateFormat, f => f.Random.String())
                .RuleFor(x => x.LongDateFormat, f => f.Random.String())
                .RuleFor(x => x.TimeFormat, f => f.Random.String())
                .RuleFor(x => x.TimeZone, f => f.Random.String())
                .RuleFor(x => x.Language, f => f.Random.String(1, 4))
                .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
        }

        public static string GetValidJsonSettings()
        {
            return typeof(UserSettingsFakeData).GetAssembly().GetResourceAsString("MockData", "ValidJsonSettings.json");
        }
    }
}
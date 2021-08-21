using System;
using Bogus;
using PlexRipper.Domain;
using PlexRipper.Settings.Models;

namespace Settings.UnitTests.MockData
{
    public static class UserSettingsFakeData
    {
        private static readonly Random _random = new();

        public static string JsonSettings =>
            "{\"FirstTimeSetup\":false,\"AccountSettings\":{\"ActiveAccountId\":999},\"AdvancedSettings\":{\"DownloadManagerSettings\":{\"DownloadSegments\":40}},\"UserInterfaceSettings\":{\"ConfirmationSettings\":{\"AskDownloadMovieConfirmation\":true,\"AskDownloadTvShowConfirmation\":true,\"AskDownloadSeasonConfirmation\":true,\"AskDownloadEpisodeConfirmation\":true},\"DisplaySettings\":{\"TvShowViewMode\":\"Poster\",\"MovieViewMode\":\"Poster\"},\"DateTimeSettings\":{\"ShortDateFormat\":\"dd\\/MM\\/yyyy\",\"LongDateFormat\":\"EEEE, dd MMMM yyyy\",\"TimeFormat\":\"HH:MM:ss\",\"TimeZone\":\"UTC\",\"ShowRelativeDates\":true}}}";


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
                .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
        }
    }
}
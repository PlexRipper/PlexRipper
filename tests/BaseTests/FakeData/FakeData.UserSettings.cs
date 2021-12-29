using System.Collections.Generic;
using System.Text.Json;
using Bogus;
using PlexRipper.Domain;
using PlexRipper.Domain.Config;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;

namespace PlexRipper.BaseTests
{
    public static partial class FakeData
    {
        public static Faker<SettingsModel> GetSettingsModel(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<SettingsModel>()
                .StrictMode(true)
                .UseSeed(config.Seed)
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
                .RuleFor(x => x.DownloadSpeedLimit, _ => new List<DownloadSpeedLimitModel>())
                .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
        }

        public static string GetSettingsModelJson(UnitTestDataConfig config = null)
        {
            var settings = GetSettingsModel(config).Generate();
            return JsonSerializer.Serialize(settings, DefaultJsonSerializerOptions.Config);
        }
    }
}
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
        private static readonly string[] ShortDateFormat = { "MMM dd yyyy", "dd MMM yyyy", "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

        private static readonly string[] LongDateFormat = { "EEEE, MMMM dd, yyyy", "EEEE, dd MMMM yyyy" };

        private static readonly string[] TimeFormat = { "HH:mm:ss", "pp" };

        public static Faker<SettingsModel> GetSettingsModel(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<SettingsModel>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.GeneralSettings, f => GetGeneralSettings(config).Generate())
                .RuleFor(x => x.ConfirmationSettings, f => GetConfirmationSettings(config).Generate())
                .RuleFor(x => x.DateTimeSettings, f => GetDateTimeSettings(config).Generate())
                .RuleFor(x => x.DisplaySettings, f => GetDisplaySettings(config).Generate())
                .RuleFor(x => x.DownloadManagerSettings, f => GetDownloadManagerSettings(config).Generate())
                .RuleFor(x => x.LanguageSettings, f => GetLanguageSettings(config).Generate())
                .RuleFor(x => x.ServerSettings, f => GetServerSettings(config).Generate());
        }

        public static Faker<GeneralSettings> GetGeneralSettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<GeneralSettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.FirstTimeSetup, f => f.Random.Bool());
        }

        public static Faker<ConfirmationSettings> GetConfirmationSettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<ConfirmationSettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.AskDownloadMovieConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.AskDownloadTvShowConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.AskDownloadSeasonConfirmation, f => f.Random.Bool())
                .RuleFor(x => x.AskDownloadEpisodeConfirmation, f => f.Random.Bool());
        }

        public static Faker<DateTimeSettings> GetDateTimeSettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<DateTimeSettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.ShortDateFormat, f => f.PickRandom(ShortDateFormat))
                .RuleFor(x => x.LongDateFormat, f => f.PickRandom(LongDateFormat))
                .RuleFor(x => x.TimeFormat, f => f.PickRandom(TimeFormat))
                .RuleFor(x => x.TimeZone, f => f.Date.TimeZoneString())
                .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
        }

        public static Faker<DisplaySettings> GetDisplaySettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<DisplaySettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.MovieViewMode, f => f.Random.Enum<ViewMode>())
                .RuleFor(x => x.TvShowViewMode, f => f.Random.Enum<ViewMode>());
        }

        public static Faker<DownloadManagerSettings> GetDownloadManagerSettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<DownloadManagerSettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.DownloadSegments, f => f.Random.Int(1, 3));
        }

        public static Faker<LanguageSettings> GetLanguageSettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<LanguageSettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Language, f => f.Random.String(2));
        }

        public static Faker<ServerSettings> GetServerSettings(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<ServerSettings>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Data, _ => new List<PlexServerSettingsModel>());
        }

        public static string GetSettingsModelJson(UnitTestDataConfig config = null)
        {
            var settings = GetSettingsModel(config).Generate();
            return JsonSerializer.Serialize(settings, DefaultJsonSerializerOptions.ConfigCaptialized);
        }
    }
}
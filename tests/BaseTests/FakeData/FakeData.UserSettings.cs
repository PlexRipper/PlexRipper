using System.Text.Json;
using Bogus;
using PlexRipper.Domain.Config;
using PlexRipper.Domain.DownloadManager;
using PlexRipper.Settings.Models;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly string[] ShortDateFormat = { "MMM dd yyyy", "dd MMM yyyy", "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd" };

    private static readonly string[] LongDateFormat = { "EEEE, MMMM dd, yyyy", "EEEE, dd MMMM yyyy" };

    private static readonly string[] TimeFormat = { "HH:mm:ss", "pp" };

    public static Faker<SettingsModel> GetSettingsModel([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<SettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.GeneralSettings, f => GetGeneralSettings(options).Generate())
            .RuleFor(x => x.ConfirmationSettings, f => GetConfirmationSettings(options).Generate())
            .RuleFor(x => x.DateTimeSettings, f => GetDateTimeSettings(options).Generate())
            .RuleFor(x => x.DisplaySettings, f => GetDisplaySettings(options).Generate())
            .RuleFor(x => x.DownloadManagerSettings, f => GetDownloadManagerSettings(options).Generate())
            .RuleFor(x => x.LanguageSettings, f => GetLanguageSettings(options).Generate())
            .RuleFor(x => x.ServerSettings, f => GetServerSettings(options).Generate());
    }

    public static string GetSettingsModelJson([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var settings = GetSettingsModel(options).Generate();
        return JsonSerializer.Serialize(settings, DefaultJsonSerializerOptions.ConfigCaptialized);
    }

    public static JsonElement GetSettingsModelJsonElement([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var settingsJson = GetSettingsModelJson(options);
        return JsonSerializer.Deserialize<JsonElement>(settingsJson, DefaultJsonSerializerOptions.ConfigCaptialized);
    }

    public static Faker<GeneralSettings> GetGeneralSettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<GeneralSettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.FirstTimeSetup, f => f.Random.Bool())
            .RuleFor(x => x.ActiveAccountId, f => f.Random.Int(1, 10));
    }

    public static Faker<ConfirmationSettings> GetConfirmationSettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<ConfirmationSettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.AskDownloadMovieConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadTvShowConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadSeasonConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadEpisodeConfirmation, f => f.Random.Bool());
    }

    public static Faker<DateTimeSettings> GetDateTimeSettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DateTimeSettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.ShortDateFormat, f => f.PickRandom(ShortDateFormat))
            .RuleFor(x => x.LongDateFormat, f => f.PickRandom(LongDateFormat))
            .RuleFor(x => x.TimeFormat, f => f.PickRandom(TimeFormat))
            .RuleFor(x => x.TimeZone, f => f.Date.TimeZoneString())
            .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
    }

    public static Faker<DisplaySettings> GetDisplaySettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DisplaySettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.MovieViewMode, f => f.Random.Enum<ViewMode>())
            .RuleFor(x => x.TvShowViewMode, f => f.Random.Enum<ViewMode>());
    }

    public static Faker<DownloadManagerSettings> GetDownloadManagerSettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DownloadManagerSettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.DownloadSegments, f => f.Random.Int(1, 3));
    }

    public static Faker<LanguageSettings> GetLanguageSettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<LanguageSettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Language, f => f.Random.String(2));
    }

    public static Faker<ServerSettings> GetServerSettings([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<ServerSettings>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Data, _ => GetPlexServerSettingsModel(options).Generate(config.PlexServerSettingsCount));
    }

    public static Faker<PlexServerSettingsModel> GetPlexServerSettingsModel([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<PlexServerSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.PlexServerId, f => f.Random.Number(int.MaxValue))
            .RuleFor(x => x.DownloadSpeedLimit, _ => config.DownloadSpeedLimit)
            .RuleFor(x => x.MachineIdentifier, f => f.Finance.BitcoinAddress());
    }
}
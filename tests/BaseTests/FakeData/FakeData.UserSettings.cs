using System.Text.Json;
using Bogus;
using PlexRipper.Domain.Config;
using Settings.Contracts;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly string[] ShortDateFormat =
    [
        "MMM dd yyyy",
        "dd MMM yyyy",
        "MM/dd/yyyy",
        "dd/MM/yyyy",
        "yyyy-MM-dd",
    ];

    private static readonly string[] LongDateFormat = ["EEEE, MMMM dd, yyyy", "EEEE, dd MMMM yyyy"];

    private static readonly string[] TimeFormat = ["HH:mm:ss", "pp"];

    public static Faker<SettingsModel> GetSettingsModel(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<SettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.GeneralSettingsModel, _ => GetGeneralSettings(options).Generate())
            .RuleFor(x => x.ConfirmationSettingsModel, _ => GetConfirmationSettings(options).Generate())
            .RuleFor(x => x.DateTimeSettingsModel, _ => GetDateTimeSettings(options).Generate())
            .RuleFor(x => x.DisplaySettingsModel, _ => GetDisplaySettings(options).Generate())
            .RuleFor(x => x.DownloadManagerSettingsModel, _ => GetDownloadManagerSettings(options).Generate())
            .RuleFor(x => x.LanguageSettingsModel, _ => GetLanguageSettings(options).Generate())
            .RuleFor(x => x.DebugSettingsModel, _ => GetDebugSettings(options).Generate())
            .RuleFor(x => x.ServerSettingsModel, _ => GetServerSettings(options).Generate());
    }

    public static string GetSettingsModelJson(Action<UnitTestDataConfig>? options = null)
    {
        var settings = GetSettingsModel(options).Generate();
        return JsonSerializer.Serialize(settings, DefaultJsonSerializerOptions.ConfigCapitalized);
    }

    public static Faker<GeneralSettingsModel> GetGeneralSettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<GeneralSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.FirstTimeSetup, f => f.Random.Bool())
            .RuleFor(x => x.DebugMode, f => f.Random.Bool())
            .RuleFor(x => x.DisableAnimatedBackground, f => f.Random.Bool())
            .RuleFor(x => x.ActiveAccountId, f => f.Random.Int(1, 10));
    }

    public static Faker<ConfirmationSettingsModel> GetConfirmationSettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<ConfirmationSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.AskDownloadMovieConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadTvShowConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadSeasonConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadEpisodeConfirmation, f => f.Random.Bool());
    }

    public static Faker<DateTimeSettingsModel> GetDateTimeSettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DateTimeSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.ShortDateFormat, f => f.PickRandom(ShortDateFormat))
            .RuleFor(x => x.LongDateFormat, f => f.PickRandom(LongDateFormat))
            .RuleFor(x => x.TimeFormat, f => f.PickRandom(TimeFormat))
            .RuleFor(x => x.TimeZone, f => f.Date.TimeZoneString())
            .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
    }

    public static Faker<DisplaySettingsModel> GetDisplaySettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DisplaySettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.MovieViewMode, f => f.Random.Enum<ViewMode>())
            .RuleFor(x => x.TvShowViewMode, f => f.Random.Enum<ViewMode>());
    }

    public static Faker<DownloadManagerSettingsModel> GetDownloadManagerSettings(
        Action<UnitTestDataConfig>? options = null
    )
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DownloadManagerSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.DownloadSegments, f => f.Random.Int(1, 3));
    }

    public static Faker<LanguageSettingsModel> GetLanguageSettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<LanguageSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Language, f => f.Random.String(2));
    }

    public static Faker<DebugSettingsModel> GetDebugSettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<DebugSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.DebugModeEnabled, f => f.Random.Bool())
            .RuleFor(x => x.MaskServerNames, f => f.Random.Bool())
            .RuleFor(x => x.MaskLibraryNames, f => f.Random.Bool());
    }

    public static Faker<ServerSettingsModel> GetServerSettings(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<ServerSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Data, _ => GetPlexServerSettingsModel(options).Generate(config.PlexServerSettingsCount));
    }

    public static Faker<PlexServerSettingsModel> GetPlexServerSettingsModel(Action<UnitTestDataConfig>? options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<PlexServerSettingsModel>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.MachineIdentifier, f => f.Finance.BitcoinAddress())
            .RuleFor(x => x.PlexServerName, _ => string.Empty)
            .RuleFor(x => x.DownloadSpeedLimit, _ => config.DownloadSpeedLimitInKib)
            .RuleFor(x => x.Hidden, _ => false);
    }
}

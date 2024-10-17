using Bogus;
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

    public static Faker<UserSettings> GetSettingsModel(Seed seed, Action<UnitTestDataConfig>? options = null)
    {
        return new Faker<UserSettings>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.GeneralSettings, _ => GetGeneralSettings(seed, options).Generate())
            .RuleFor(x => x.ConfirmationSettings, _ => GetConfirmationSettings(seed, options).Generate())
            .RuleFor(x => x.DateTimeSettings, _ => GetDateTimeSettings(seed, options).Generate())
            .RuleFor(x => x.DisplaySettings, _ => GetDisplaySettings(seed, options).Generate())
            .RuleFor(x => x.DownloadManagerSettings, _ => GetDownloadManagerSettings(seed, options).Generate())
            .RuleFor(x => x.LanguageSettings, _ => GetLanguageSettings(seed, options).Generate())
            .RuleFor(x => x.DebugSettings, _ => GetDebugSettings(seed, options).Generate())
            .RuleFor(x => x.ServerSettings, _ => GetServerSettings(seed, options).Generate());
    }

    public static Faker<GeneralSettingsModule> GetGeneralSettings(Seed seed, Action<UnitTestDataConfig>? options = null)
    {
        return new Faker<GeneralSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.FirstTimeSetup, f => f.Random.Bool())
            .RuleFor(x => x.DebugMode, f => f.Random.Bool())
            .RuleFor(x => x.DisableAnimatedBackground, f => f.Random.Bool())
            .RuleFor(x => x.ActiveAccountId, f => f.Random.Int(1, 10));
    }

    public static Faker<ConfirmationSettingsModule> GetConfirmationSettings(
        Seed seed,
        Action<UnitTestDataConfig>? options = null
    )
    {
        return new Faker<ConfirmationSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.AskDownloadMovieConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadTvShowConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadSeasonConfirmation, f => f.Random.Bool())
            .RuleFor(x => x.AskDownloadEpisodeConfirmation, f => f.Random.Bool());
    }

    public static Faker<DateTimeSettingsModule> GetDateTimeSettings(
        Seed seed,
        Action<UnitTestDataConfig>? options = null
    )
    {
        return new Faker<DateTimeSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.ShortDateFormat, f => f.PickRandom(ShortDateFormat))
            .RuleFor(x => x.LongDateFormat, f => f.PickRandom(LongDateFormat))
            .RuleFor(x => x.TimeFormat, f => f.PickRandom(TimeFormat))
            .RuleFor(x => x.TimeZone, f => f.Date.TimeZoneString())
            .RuleFor(x => x.ShowRelativeDates, f => f.Random.Bool());
    }

    public static Faker<DisplaySettingsModule> GetDisplaySettings(Seed seed, Action<UnitTestDataConfig>? options = null)
    {
        return new Faker<DisplaySettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.MovieViewMode, f => f.Random.Enum<ViewMode>())
            .RuleFor(x => x.TvShowViewMode, f => f.Random.Enum<ViewMode>());
    }

    public static Faker<DownloadManagerSettingsModule> GetDownloadManagerSettings(
        Seed seed,
        Action<UnitTestDataConfig>? options = null
    )
    {
        return new Faker<DownloadManagerSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.DownloadSegments, f => f.Random.Int(1, 3));
    }

    public static Faker<LanguageSettingsModule> GetLanguageSettings(
        Seed seed,
        Action<UnitTestDataConfig>? options = null
    )
    {
        return new Faker<LanguageSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Language, f => f.Random.String(2));
    }

    public static Faker<DebugSettingsModule> GetDebugSettings(Seed seed, Action<UnitTestDataConfig>? options = null)
    {
        return new Faker<DebugSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.DebugModeEnabled, f => f.Random.Bool())
            .RuleFor(x => x.MaskServerNames, f => f.Random.Bool())
            .RuleFor(x => x.MaskLibraryNames, f => f.Random.Bool());
    }

    public static Faker<PlexServerSettingsModule> GetServerSettings(
        Seed seed,
        Action<UnitTestDataConfig>? options = null
    )
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<PlexServerSettingsModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(
                x => x.Data,
                _ => GetPlexServerSettingsModel(seed, options).Generate(config.PlexServerSettingsCount)
            );
    }

    public static Faker<PlexServerSettingItemModule> GetPlexServerSettingsModel(
        Seed seed,
        Action<UnitTestDataConfig>? options = null
    )
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<PlexServerSettingItemModule>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.MachineIdentifier, f => f.Finance.BitcoinAddress())
            .RuleFor(x => x.PlexServerName, _ => string.Empty)
            .RuleFor(x => x.DownloadSpeedLimit, _ => config.DownloadSpeedLimitInKib)
            .RuleFor(x => x.Hidden, _ => false);
    }
}

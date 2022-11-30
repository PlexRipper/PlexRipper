using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly Random _random = new();

    public static Faker<PlexServer> GetPlexServer([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        return new Faker<PlexServer>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(p => p.Id, _ => 0)
            .RuleFor(x => x.Name, f => f.Company.CompanyName())
            .RuleFor(x => x.Product, _ => "Plex Media Server")
            .RuleFor(x => x.ProductVersion, f => f.System.Semver())
            .RuleFor(x => x.Platform, _ => "Linux")
            .RuleFor(x => x.PlatformVersion, f => f.System.Semver())
            .RuleFor(x => x.Device, f => f.Company.CompanyName())
            .RuleFor(x => x.MachineIdentifier, _ => Guid.NewGuid().ToString())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.Now))
            .RuleFor(x => x.LastSeenAt, f => f.Date.Recent(30))
            .RuleFor(x => x.Provides, f => f.Company.CompanyName())
            .RuleFor(x => x.OwnerId, f => f.Random.Int(1000, 100000))
            .RuleFor(x => x.PlexServerOwnerUsername, f => f.Name.LastName())
            .RuleFor(x => x.PublicAddress, f => f.Internet.Ip())
            .RuleFor(x => x.AccessToken, _ => "DO NOT USE")

            // Server flags
            .RuleFor(x => x.Owned, f => f.Random.Bool())
            .RuleFor(x => x.Home, f => f.Random.Bool())
            .RuleFor(x => x.Synced, f => f.Random.Bool())
            .RuleFor(x => x.Relay, f => f.Random.Bool())
            .RuleFor(x => x.Presence, f => f.Random.Bool())
            .RuleFor(x => x.HttpsRequired, f => f.Random.Bool())
            .RuleFor(x => x.PublicAddressMatches, f => f.Random.Bool())
            .RuleFor(x => x.DnsRebindingProtection, f => f.Random.Bool())
            .RuleFor(x => x.NatLoopbackSupported, f => f.Random.Bool())
            .RuleFor(x => x.PlexServerConnections, GetPlexServerConnections(options).Generate(3))
            .RuleFor(x => x.PlexLibraries, _ => new List<PlexLibrary>())
            .RuleFor(x => x.ServerStatus, _ => new List<PlexServerStatus>())
            .RuleFor(x => x.PlexAccountServers, _ => new List<PlexAccountServer>());
    }

    public static Faker<PlexLibrary> GetPlexLibrary([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options, new UnitTestDataConfig
        {
            LibraryType = PlexMediaType.Movie,
        });

        return new Faker<PlexLibrary>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.Key, f => f.Random.Int(1, 10000).ToString())
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.Type, _ => config.LibraryType)
            .RuleFor(x => x.PlexServerId, f => f.Random.Int(1, 10000))
            .RuleFor(x => x.PlexServer, _ => new PlexServer())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(4))
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent())
            .RuleFor(x => x.ScannedAt, f => f.Date.Recent())
            .RuleFor(x => x.SyncedAt, f => f.Date.Recent())
            .RuleFor(x => x.Uuid, _ => Guid.NewGuid())
            .RuleFor(x => x.LibraryLocationId, f => f.Random.Int(1, 10000))
            .RuleFor(x => x.LibraryLocationPath, f => f.System.DirectoryPath())
            .RuleFor(x => x.MetaData, _ => new PlexLibraryMetaData())
            .RuleFor(x => x.DefaultDestination, _ => new FolderPath())
            .RuleFor(x => x.DefaultDestinationId, f => f.Random.Int(1, 5))
            .RuleFor(x => x.Movies, _ => new List<PlexMovie>())
            .RuleFor(x => x.TvShows, _ => new List<PlexTvShow>())
            .RuleFor(x => x.PlexAccountLibraries, _ => new List<PlexAccountLibrary>())
            .RuleFor(x => x.DownloadTasks, _ => new List<DownloadTask>());
    }

    public static Faker<FolderPath> GetFolderPaths([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        var ids = 0;
        return new Faker<FolderPath>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Id, _ => ids++)
            .RuleFor(x => x.DisplayName, f => f.Random.Word())
            .RuleFor(x => x.FolderType, f => f.Random.Enum<FolderType>())
            .RuleFor(x => x.MediaType, f => f.Random.Enum<PlexMediaType>())
            .RuleFor(x => x.DirectoryPath, f => f.System.DirectoryPath())
            .RuleFor(x => x.PlexLibraries, _ => new List<PlexLibrary>());
    }

    public static Faker<PlexServerConnection> GetPlexServerConnections([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);
        var uri = config.MockServer?.ServerUri ?? new Uri("https://test-server.com");

        var ids = 0;
        return new Faker<PlexServerConnection>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Id, _ => ids++)
            .RuleFor(x => x.Protocol, _ => uri.Scheme)
            .RuleFor(x => x.Address, _ => uri.Host)
            .RuleFor(x => x.Port, _ => uri.Port)
            .RuleFor(x => x.Local, _ => false)
            .RuleFor(x => x.Relay, _ => false)
            .RuleFor(x => x.IPv6, _ => false);
    }


    private static int GetUniqueId(List<int> alreadyGenerated, [CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        var config = UnitTestDataConfig.FromOptions(options);

        var rnd = new Random(config.Seed);
        while (true)
        {
            var value = rnd.Next(1, 10000000);
            if (!alreadyGenerated.Contains(value))
            {
                alreadyGenerated.Add(value);
                return value;
            }
        }
    }
}
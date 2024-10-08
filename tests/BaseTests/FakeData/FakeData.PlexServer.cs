using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    public static Faker<PlexServer> GetPlexServer(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        // Note: Ensure all faker values are a lambda f => x,
        // otherwise Entity Framework will see differently generated values as the same object and mess up any database testing
        return new Faker<PlexServer>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(p => p.Id, _ => 0)
            .RuleFor(x => x.Name, f => f.Company.CompanyName())
            .RuleFor(x => x.Product, _ => "Plex Media Server")
            .RuleFor(x => x.ProductVersion, f => f.System.Semver())
            .RuleFor(x => x.Platform, _ => "Linux")
            .RuleFor(x => x.PlatformVersion, f => f.System.Semver())
            .RuleFor(x => x.Device, f => f.Company.CompanyName())
            .RuleFor(x => x.MachineIdentifier, f => f.Random.Guid().ToString())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.UtcNow))
            .RuleFor(x => x.LastSeenAt, f => f.Date.Recent(30))
            .RuleFor(x => x.Provides, f => f.Company.CompanyName())
            .RuleFor(x => x.OwnerId, f => f.Random.Int(1000, 100000))
            .RuleFor(x => x.PlexServerOwnerUsername, f => f.Name.LastName())
            .RuleFor(x => x.PublicAddress, f => f.Internet.Ip())
            .RuleFor(x => x.IsEnabled, _ => true)
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
            .RuleFor(x => x.PreferredConnectionId, _ => 0)
            .RuleFor(
                x => x.PlexServerConnections,
                _ => GetPlexServerConnections(seed).Generate(config.PlexServerConnectionPerServerCount)
            )
            .RuleFor(x => x.PlexLibraries, _ => [])
            .RuleFor(x => x.ServerStatus, _ => [])
            .RuleFor(x => x.ServerFixApplyDNSFix, _ => false)
            .RuleFor(x => x.PlexAccountServers, _ => []);
    }

    public static Faker<PlexLibrary> GetPlexLibrary(int seed = 0, PlexMediaType libraryType = PlexMediaType.None)
    {
        return new Faker<PlexLibrary>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.Key, _ => GetUniqueNumber().ToString())
            .RuleFor(x => x.Title, f => f.Company.CompanyName())
            .RuleFor(x => x.Type, f => libraryType == PlexMediaType.None ? f.PlexRipper().LibraryType : libraryType)
            .RuleFor(x => x.PlexServerId, _ => GetUniqueNumber())
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(4))
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent())
            .RuleFor(x => x.ScannedAt, f => f.Date.Recent())
            .RuleFor(x => x.SyncedAt, f => f.Date.Recent())
            .RuleFor(x => x.Uuid, _ => Guid.NewGuid())
            .RuleFor(x => x.DefaultDestination, _ => null)
            .RuleFor(x => x.DefaultDestinationId, _ => null)
            .RuleFor(x => x.Movies, _ => [])
            .RuleFor(x => x.TvShows, _ => [])
            .RuleFor(x => x.PlexAccountLibraries, _ => []);
    }

    public static Faker<FolderPath> GetFolderPaths(int seed = 0)
    {
        var ids = 0;
        return new Faker<FolderPath>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => ids++)
            .RuleFor(x => x.DisplayName, f => f.Random.Word())
            .RuleFor(x => x.FolderType, f => f.Random.Enum<FolderType>())
            .RuleFor(x => x.MediaType, f => f.Random.Enum<PlexMediaType>())
            .RuleFor(x => x.DirectoryPath, f => f.System.DirectoryPath())
            .RuleFor(x => x.PlexLibraries, _ => []);
    }

    public static Faker<PlexServerConnection> GetPlexServerConnections(int seed = 0)
    {
        return new Faker<PlexServerConnection>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.Protocol, f => f.Internet.Protocol())
            .RuleFor(x => x.Address, f => f.Internet.Ip())
            .RuleFor(x => x.Port, f => f.Internet.Port())
            .RuleFor(x => x.Local, _ => false)
            .RuleFor(x => x.Relay, _ => false)
            .RuleFor(x => x.IPv4, _ => true)
            .RuleFor(x => x.IPv6, _ => false)
            .RuleFor(x => x.PortFix, _ => false)
            .RuleFor(x => x.Uri, f => f.Internet.Url())
            .RuleFor(x => x.PlexServerStatus, _ => [])
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexServerId, _ => 0)
            .FinishWith(
                (_, connection) =>
                {
                    connection.Uri = $"{connection.Protocol}://{connection.Address}:{connection.Port}";
                }
            );
    }

    public static Faker<PlexServerStatus> GetPlexServerStatus(int seed = 0)
    {
        return new Faker<PlexServerStatus>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.IsSuccessful, _ => true)
            .RuleFor(x => x.StatusCode, _ => 200)
            .RuleFor(x => x.StatusMessage, f => f.Hacker.Phrase())
            .RuleFor(x => x.LastChecked, f => f.Date.Recent())
            .RuleFor(x => x.PlexServerConnection, _ => null)
            .RuleFor(x => x.PlexServerConnectionId, _ => 0)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexServerId, _ => 0);
    }

    public static List<PlexAccountServer> GetPlexAccountServer(
        PlexAccount plexAccount,
        List<PlexServer> plexServers,
        int seed = 0
    )
    {
        var index = 0;
        return new Faker<PlexAccountServer>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.PlexAccountId, _ => plexAccount.Id)
            .RuleFor(x => x.PlexAccount, _ => null)
            .RuleFor(x => x.PlexServerId, _ => plexServers[index++].Id)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.AuthToken, f => f.Random.Uuid().ToString())
            .RuleFor(x => x.AuthTokenCreationDate, _ => DateTime.UtcNow)
            .Generate(plexServers.Count);
    }

    public static List<ServerAccessTokenDTO> GetServerAccessTokenDTO(
        PlexAccount plexAccount,
        List<PlexServer> plexServers,
        int seed = 0
    )
    {
        var index = 0;
        return new Faker<ServerAccessTokenDTO>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.PlexAccountId, _ => plexAccount.Id)
            .RuleFor(x => x.MachineIdentifier, _ => plexServers[index++].MachineIdentifier)
            .RuleFor(x => x.AccessToken, f => f.Random.Uuid().ToString())
            .Generate(plexServers.Count);
    }
}

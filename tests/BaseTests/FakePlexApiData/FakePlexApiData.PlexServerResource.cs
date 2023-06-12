using Bogus;
using PlexRipper.PlexApi.Api;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    #region Methods

    #region Public

    public static Faker<ServerResource> GetServerResource(Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<ServerResource>()
            .StrictMode(true)
            .UseSeed(config.Seed)
            .RuleFor(x => x.Name, f => f.Company.CompanyName())
            .RuleFor(x => x.Product, _ => "Plex Media Server")
            .RuleFor(x => x.ProductVersion, f => f.System.Semver())
            .RuleFor(x => x.Platform, _ => "Linux")
            .RuleFor(x => x.PlatformVersion, f => f.System.Semver())
            .RuleFor(x => x.Device, f => f.PlexApi().Device)
            .RuleFor(x => x.ClientIdentifier, f => f.PlexApi().ClientId)
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.UtcNow))
            .RuleFor(x => x.LastSeenAt, f => f.Date.Recent(30))
            .RuleFor(x => x.Provides, _ => "server")
            .RuleFor(x => x.OwnerId, f => f.Random.Int(1000, 100000))
            .RuleFor(x => x.SourceTitle, f => f.Internet.UserName())
            .RuleFor(x => x.PublicAddress, f => f.Internet.Ip())
            .RuleFor(x => x.AccessToken, f => f.PlexApi().AccessToken)
            .RuleFor(x => x.Owned, f => f.Random.Bool())
            .RuleFor(x => x.Home, f => f.Random.Bool())
            .RuleFor(x => x.Synced, f => f.Random.Bool())
            .RuleFor(x => x.Relay, f => f.Random.Bool())
            .RuleFor(x => x.Presence, f => f.Random.Bool())
            .RuleFor(x => x.HttpsRequired, f => f.Random.Bool())
            .RuleFor(x => x.PublicAddressMatches, f => f.Random.Bool())
            .RuleFor(x => x.DnsRebindingProtection, f => f.Random.Bool())
            .RuleFor(x => x.NatLoopbackSupported, f => f.Random.Bool())
            .RuleFor(x => x.Connections, f => GetPlexServerResourceConnections(options).Generate(f.Random.Int(1, 4)));
    }

    public static Faker<ServerResourceConnection> GetPlexServerResourceConnections(Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<ServerResourceConnection>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
            .RuleFor(x => x.Protocol, _ => "http")

            // This has to be an ip otherwise the PortFix gets activated in PlexServerConnection
            .RuleFor(x => x.Address, _ => "240.0.0.0")
            .RuleFor(x => x.Port, f => f.Internet.Port())
            .RuleFor(x => x.Uri, _ => "")
            .RuleFor(x => x.Local, _ => false)
            .RuleFor(x => x.Relay, _ => false)
            .RuleFor(x => x.IPv6, _ => false)
            .FinishWith((_, connection) => { connection.Uri = new UriBuilder(connection.Protocol, connection.Address, connection.Port).ToString(); });
    }

    #endregion

    #endregion
}
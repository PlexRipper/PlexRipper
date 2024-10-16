using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.BaseTests;

public partial class FakePlexApiData
{
    #region Methods

    #region Public

    public static Faker<PlexDevice> GetServerResource(Action<PlexApiDataConfig>? options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        return new Faker<PlexDevice>()
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
            .RuleFor(
                x => x.Connections,
                _ => GetPlexServerResourceConnections(options).Generate(config.PlexServerAccessConnectionsCount)
            );
    }

    public static Faker<Connections> GetPlexServerResourceConnections(Action<PlexApiDataConfig>? options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        var https = config.PlexServerAccessConnectionsIncludeHttps;
        return new Faker<Connections>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
            .RuleFor(x => x.Protocol, _ => !https ? Protocol.Http : Protocol.Https)
            .RuleFor(x => x.Address, f => f.Internet.Ip())
            .RuleFor(x => x.Port, f => f.Internet.Port())
            .RuleFor(
                x => x.Uri,
                (f, connection) =>
                {
                    if (https)
                    {
                        var address = connection.Address.Replace('.', '-');
                        return $"https://{address}.{f.Random.Hexadecimal(32, string.Empty).ToLower()}.plex.direct:{connection.Port}";
                    }

                    return new UriBuilder("http", connection.Address, connection.Port).ToString();
                }
            )
            .RuleFor(x => x.Local, _ => false)
            .RuleFor(x => x.Relay, _ => false)
            .RuleFor(x => x.IPv6, _ => false);
    }

    #endregion

    #endregion
}

using Bogus;
using PlexRipper.PlexApi.Api;

namespace PlexRipper.BaseTests;

public static partial class FakePlexApiData
{
    public static ServerIdentityResponse GetPlexServerIdentityResponse([CanBeNull] Action<PlexApiDataConfig> options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        var container = new Faker<ServerIdentityResponseMediaContainer>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
            .RuleFor(x => x.Size, _ => 0)
            .RuleFor(x => x.Claimed, f => f.Random.Bool())
            .RuleFor(x => x.MachineIdentifier, f => f.PlexApi().MachineIdentifier)
            .RuleFor(x => x.Version, f => f.PlexApi().PlexVersion);

        return new Faker<ServerIdentityResponse>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
            .RuleFor(x => x.MediaContainer, _ => container.Generate())
            .Generate();
    }
}
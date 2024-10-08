using Bogus;
using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.BaseTests;

public static partial class FakePlexApiData
{
    public static GetServerIdentityResponseBody GetPlexServerIdentityResponse(Action<PlexApiDataConfig>? options = null)
    {
        var config = PlexApiDataConfig.FromOptions(options);

        var container = new Faker<GetServerIdentityMediaContainer>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
            .RuleFor(x => x.Size, _ => 0)
            .RuleFor(x => x.Claimed, f => f.Random.Bool())
            .RuleFor(x => x.MachineIdentifier, f => f.PlexApi().MachineIdentifier)
            .RuleFor(x => x.Version, f => f.PlexApi().PlexVersion);

        return new Faker<GetServerIdentityResponseBody>()
            .StrictMode(true)
            .UseSeed(config.GetSeed())
            .RuleFor(x => x.MediaContainer, _ => container.Generate())
            .Generate();
    }
}

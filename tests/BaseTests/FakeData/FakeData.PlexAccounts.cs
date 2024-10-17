using Bogus;

namespace PlexRipper.BaseTests;

public partial class FakeData
{
    public static Faker<PlexAccount> GetPlexAccount(int seed) => GetPlexAccount(new Seed(seed));

    public static Faker<PlexAccount> GetPlexAccount(Seed seed)
    {
        return new Faker<PlexAccount>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.DisplayName, f => f.Internet.UserName())
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Password, f => f.Internet.Password())
            .RuleFor(x => x.Email, f => f.Internet.Email())
            .RuleFor(x => x.IsEnabled, _ => true)
            .RuleFor(x => x.IsValidated, _ => true)
            .RuleFor(x => x.ValidatedAt, f => f.Date.Recent())
            .RuleFor(x => x.PlexId, f => f.Random.Long(1, 10000))
            .RuleFor(x => x.Uuid, f => f.Random.Guid().ToString())
            .RuleFor(x => x.ClientId, f => f.Random.Guid().ToString())
            .RuleFor(x => x.Title, f => f.Internet.UserName())
            .RuleFor(x => x.HasPassword, _ => true)
            .RuleFor(x => x.AuthenticationToken, f => f.Random.Guid().ToString())
            .RuleFor(x => x.IsMain, _ => true)
            .RuleFor(x => x.Is2Fa, _ => false)
            .RuleFor(x => x.IsAuthTokenMode, _ => false)
            .RuleFor(x => x.VerificationCode, _ => "")
            .RuleFor(x => x.PlexAccountServers, _ => new List<PlexAccountServer>())
            .RuleFor(x => x.PlexAccountLibraries, _ => new List<PlexAccountLibrary>());
    }
}

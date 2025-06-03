namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static Faker<PlexCountry> _plexCountry = new Faker<PlexCountry>()
        .StrictMode(true)
        .RuleFor(x => x.Id, _ => 0)
        .RuleFor(x => x.PlexKey, _ => GetUniqueNumber())
        .RuleFor(x => x.Name, f => f.Address.Country())
        .Ignore(x => x.PlexLibraries)
        .Ignore(x => x.PlexMovieCountries)
        .Ignore(x => x.PlexTvShowCountries);

    public static Faker<PlexCountry> GetPlexCountries(Seed seed, Action<FakeDataConfig>? options = null) =>
        _plexCountry.UseSeed(seed.Next());
}

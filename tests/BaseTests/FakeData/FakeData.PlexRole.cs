namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static Faker<PlexRole> _plexRole = new Faker<PlexRole>()
        .StrictMode(true)
        .RuleFor(x => x.Id, _ => 0)
        .RuleFor(x => x.PlexKey, _ => GetUniqueNumber())
        .RuleFor(x => x.Name, f => f.Name.FullName())
        .RuleFor(x => x.Role, f => f.Name.JobTitle())
        .RuleFor(x => x.TagKey, f => f.Random.AlphaNumeric(10))
        .RuleFor(x => x.Thumb, f => f.Image.PicsumUrl())
        .Ignore(x => x.PlexLibraries)
        .Ignore(x => x.PlexMovieRoles)
        .Ignore(x => x.PlexTvShowRoles);

    public static Faker<PlexRole> GetPlexRoles(Seed seed, Action<FakeDataConfig>? options = null) =>
        _plexRole.UseSeed(seed.Next());
}

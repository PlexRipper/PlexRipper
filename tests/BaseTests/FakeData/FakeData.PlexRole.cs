namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    private static readonly Faker<PlexActor> _plexActor = new Faker<PlexActor>()
        .StrictMode(true)
        .RuleFor(x => x.Id, _ => 0)
        .RuleFor(x => x.Name, f => f.Name.FullName())
        .RuleFor(x => x.Key, f => f.Random.Hash(24))
        .RuleFor(x => x.PlexKey, f => f.Random.AlphaNumeric(24))
        .RuleFor(x => x.Thumb, f => f.Image.PicsumUrl())
        .Ignore(x => x.PlexLibraries)
        .Ignore(x => x.PlexMovieActors)
        .Ignore(x => x.PlexTvShowActors);

    public static Faker<PlexActor> GetPlexActors(Seed seed, Action<FakeDataConfig>? options = null) =>
        _plexActor.UseSeed(seed.Next());
}

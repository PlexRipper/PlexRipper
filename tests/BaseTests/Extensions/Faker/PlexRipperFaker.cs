using Bogus;
using Bogus.Premium;

namespace PlexRipper.BaseTests;

public static class PlexRipperFaker
{
    public static PlexRipperData PlexRipper(this Faker faker)
    {
        return ContextHelper.GetOrSet(faker, () => new PlexRipperData(faker));
    }
}

public class PlexRipperData : DataSet
{
    private readonly Faker _faker;

    public PlexRipperData(Faker faker)
    {
        _faker = faker;
    }

    private static readonly List<PlexMediaType> LibraryTypes = [PlexMediaType.Movie, PlexMediaType.TvShow];

    public PlexMediaType LibraryType => _faker.Random.ListItem(LibraryTypes);
}

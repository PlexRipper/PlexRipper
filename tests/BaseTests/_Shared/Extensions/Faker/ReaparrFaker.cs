using Bogus.Premium;

namespace Reaparr.BaseTests;

public static class ReaparrFaker
{
    public static ReaparrData Reaparr(this Faker faker)
    {
        return ContextHelper.GetOrSet(faker, () => new ReaparrData(faker));
    }
}

public class ReaparrData : DataSet
{
    private readonly Faker _faker;

    public ReaparrData(Faker faker)
    {
        _faker = faker;
    }

    private static readonly List<PlexMediaType> _libraryTypes = [PlexMediaType.Movie, PlexMediaType.TvShow];

    public PlexMediaType LibraryType => _faker.Random.ListItem(_libraryTypes);
}

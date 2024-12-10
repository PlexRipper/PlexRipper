using Bogus;
using Bogus.Premium;
using PlexRipper.BaseTests.Datasets;

namespace PlexRipper.BaseTests;

public static class PlexMediaFaker
{
    public static PlexMediaDataSet PlexMedia(this Faker faker)
    {
        return ContextHelper.GetOrSet(faker, () => new PlexMediaDataSet(faker));
    }
}

public class PlexMediaDataSet : DataSet
{
    private readonly Faker _faker;

    public PlexMediaDataSet(Faker faker)
    {
        _faker = faker;
    }

    public string MediaTitle(PlexMediaType type)
    {
        if (type == PlexMediaType.Movie)
        {
            return MovieTitle;
        }

        if (type == PlexMediaType.TvShow)
        {
            return TvShowTitle;
        }

        throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown PlexMediaType");
    }

    public string MovieTitle => PlexMovieShowTitlesDataset.PlexMovieTitles.GetByIndex(_faker.Random.Int(0, 1000 - 1));

    public string TvShowTitle => PlexTvShowTitlesDataset.PlexTVShowTitles.GetByIndex(_faker.Random.Int(0, 1000 - 1));
}

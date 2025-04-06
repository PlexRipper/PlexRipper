using Bogus;
using Bogus.Premium;

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
        var index = _faker.Random.Int(0, 1000 - 1);
        return type switch
        {
            PlexMediaType.Movie => PlexMovieShowTitlesDataset.PlexMovieTitles.GetByIndex(index),
            PlexMediaType.TvShow => PlexTvShowTitlesDataset.PlexTVShowTitles.GetByIndex(index),
            PlexMediaType.Episode => PlexEpisodeShowTitlesDataset.PlexEpisodeTitles.GetByIndex(index),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "PlexMediaType not supported."),
        };
    }
}

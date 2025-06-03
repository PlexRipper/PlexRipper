using Bogus;
using Bogus.Premium;
using PlexApi.Contracts;

namespace PlexRipper.BaseTests;

public static class PlexMediaFaker
{
    // TODO make this a property when migrated to C# 14
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

    public string MediaGenre()
    {
        var index = _faker.Random.Int(0, PlexMediaGenreDataset.Count - 1);

        return PlexMediaGenreDataset.PlexMediaGenres.GetByIndex(index);
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

    public string MediaTitle(DownloadTaskType type)
    {
        var index = _faker.Random.Int(0, 1000 - 1);
        return type switch
        {
            DownloadTaskType.Movie => "Movie - " + PlexMovieShowTitlesDataset.PlexMovieTitles.GetByIndex(index),
            DownloadTaskType.MovieData => "MovieData - " + PlexMovieShowTitlesDataset.PlexMovieTitles.GetByIndex(index),
            DownloadTaskType.TvShow => "TvShow - " + PlexTvShowTitlesDataset.PlexTVShowTitles.GetByIndex(index),
            DownloadTaskType.Season => "Season",
            DownloadTaskType.Episode => "Episode - " + PlexEpisodeShowTitlesDataset.PlexEpisodeTitles.GetByIndex(index),
            DownloadTaskType.EpisodeData => "EpisodeData - "
                + PlexEpisodeShowTitlesDataset.PlexEpisodeTitles.GetByIndex(index),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "PlexMediaType not supported."),
        };
    }

    public string Guid(PlexMediaType type) =>
        $"plex://{type.ToPlexApiString()}/${_faker.Random.Guid().ToString().Replace("-", "")}";
}

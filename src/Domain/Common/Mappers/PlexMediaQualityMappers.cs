namespace PlexRipper.Domain;

public static class PlexMediaQualityMappers
{
    public static PlexMediaQuality ToPlexMediaQuality(this PlexTvShowEpisodeMediaData source) =>
        new()
        {
            Id = source.Id,
            Type = PlexMediaType.Episode,
            Quality = source.Quality,
        };

    public static PlexMediaQuality ToPlexMediaQuality(this PlexMovieMediaData source) =>
        new()
        {
            Id = source.Id,
            Type = PlexMediaType.Movie,
            Quality = source.Quality,
        };
}

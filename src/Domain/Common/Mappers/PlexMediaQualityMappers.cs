namespace PlexRipper.Domain;

public static class PlexMediaQualityMappers
{
    public static PlexMediaQuality ToPlexMediaQuality(this PlexTvShowEpisodeMediaData source) =>
        new()
        {
            DataId = source.Id,
            MediaDataType = PlexMediaType.Episode,
            Quality = source.Quality,
            MediaId = source.PlexTvShowEpisodeId,
        };

    public static PlexMediaQuality ToPlexMediaQuality(this PlexMovieMediaData source) =>
        new()
        {
            DataId = source.Id,
            MediaDataType = PlexMediaType.Movie,
            Quality = source.Quality,
            MediaId = source.PlexMovieId,
        };
}

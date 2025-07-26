using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaQualityMappers
{
    #region PlexMovieMediaQuality

    public static PlexMediaQualityDTO ToDTO(this PlexMovieMediaQuality source) =>
        new()
        {
            Quality =
                source.PlexMediaQuality?.Quality
                ?? throw new NullReferenceException(
                    $"Ensure {nameof(source.PlexMediaQuality)} is included in the query."
                ),
            DataId =
                source.PlexMovieMediaData?.Id
                ?? throw new NullReferenceException(
                    $"Ensure {nameof(source.PlexMovieMediaData)} is included in the query."
                ),
            MediaDataType = source.Type,
            MediaId = source.PlexMovieId,
        };

    public static List<PlexMediaQualityDTO> ToDTO(this IEnumerable<PlexMovieMediaQuality> source) =>
        source.Select(x => x.ToDTO()).ToList();

    #endregion

    #region PlexTvShowMediaQuality

    public static PlexMediaQualityDTO ToDTO(this PlexTvShowMediaQuality source) =>
        new()
        {
            Quality =
                source.PlexMediaQuality?.Quality
                ?? throw new NullReferenceException(
                    $"Ensure {nameof(source.PlexMediaQuality)} is included in the query."
                ),
            DataId =
                source.PlexMediaQuality?.Id
                ?? throw new NullReferenceException(
                    $"Ensure {nameof(source.PlexMediaQuality)} is included in the query."
                ),
            MediaDataType = source.Type,
            MediaId = source.PlexTvShowId,
        };

    public static List<PlexMediaQualityDTO> ToDTO(this IEnumerable<PlexTvShowMediaQuality> source) =>
        source.Select(x => x.ToDTO()).ToList();

    #endregion

    #region PlexTvShowSeasonMediaQuality

    public static PlexMediaQualityDTO ToDTO(this PlexTvShowSeasonMediaQuality source) =>
        new()
        {
            Quality =
                source.PlexMediaQuality?.Quality
                ?? throw new NullReferenceException(
                    $"Ensure {nameof(source.PlexMediaQuality)} is included in the query."
                ),
            DataId =
                source.PlexMediaQuality?.Id
                ?? throw new NullReferenceException(
                    $"Ensure {nameof(source.PlexMediaQuality)} is included in the query."
                ),
            MediaDataType = source.Type,
            MediaId = source.PlexTvShowSeasonId,
        };

    public static List<PlexMediaQualityDTO> ToDTO(this IEnumerable<PlexTvShowSeasonMediaQuality> source) =>
        source.Select(x => x.ToDTO()).ToList();

    #endregion

    #region PlexMovieMediaData

    public static PlexMediaQualityDTO ToPlexMediaQuality(this PlexMovieMediaData source) =>
        new()
        {
            DataId = source.Id,
            MediaDataType = PlexMediaType.Movie,
            Quality = source.Quality,
            MediaId = source.PlexMovieId,
        };

    public static List<PlexMediaQualityDTO> ToPlexMediaQuality(this IEnumerable<PlexMovieMediaData> source) =>
        source.Select(x => x.ToPlexMediaQuality()).ToList();

    #endregion

    #region PlexTvShowMediaQuality

    public static PlexMediaQualityDTO ToPlexMediaQuality(this PlexTvShowEpisodeMediaData source) =>
        new()
        {
            DataId = source.Id,
            MediaDataType = PlexMediaType.Episode,
            Quality = source.Quality,
            MediaId = source.PlexTvShowEpisodeId,
        };

    public static List<PlexMediaQualityDTO> ToPlexMediaQuality(this IEnumerable<PlexTvShowEpisodeMediaData> source) =>
        source.Select(x => x.ToPlexMediaQuality()).ToList();

    #endregion
}

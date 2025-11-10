using Reaparr.Domain;

namespace Reaparr.Application.Contracts;

public static class PlexMediaQualityMappers
{
    private const int NO_MEDIA_DATA_ID = -1;

    #region PlexTvShowMediaQuality

    public static PlexMediaQualityDTO ToDTO(this PlexTvShowMediaQuality source) =>
        new()
        {
            Quality = source.Quality,
            MediaId = NO_MEDIA_DATA_ID,
            DataId = NO_MEDIA_DATA_ID,
            MediaDataType = source.Type,
        };

    public static List<PlexMediaQualityDTO> ToDTO(this IEnumerable<PlexTvShowMediaQuality> source) =>
        source.SortByQuality().Select(x => x.ToDTO()).ToList();

    #endregion

    #region PlexTvShowSeasonMediaQuality

    public static PlexMediaQualityDTO ToDTO(this PlexTvShowSeasonMediaQuality source) =>
        new()
        {
            Quality = source.Quality,
            MediaDataType = source.Type,
            MediaId = source.PlexTvShowSeasonId,
            DataId = NO_MEDIA_DATA_ID,
        };

    public static List<PlexMediaQualityDTO> ToDTO(this IEnumerable<PlexTvShowSeasonMediaQuality> source) =>
        source.SortByQuality().Select(x => x.ToDTO()).ToList();

    #endregion

    #region PlexTvShowEpisodeMediaData

    public static PlexMediaQualityDTO ToPlexMediaQuality(this PlexTvShowEpisodeMediaData source) =>
        new()
        {
            DataId = source.Id,
            MediaDataType = PlexMediaType.Episode,
            Quality = source.Quality,
            MediaId = source.PlexTvShowEpisodeId,
        };

    public static List<PlexMediaQualityDTO> ToPlexMediaQuality(this IEnumerable<PlexTvShowEpisodeMediaData> source) =>
        source.SortByQuality().Select(x => x.ToPlexMediaQuality()).ToList();

    #endregion
}

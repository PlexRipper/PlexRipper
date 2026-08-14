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

    #region ToVideoQuality

    private static readonly IReadOnlyDictionary<VideoQuality, int> _videoQualityIds = new Dictionary<VideoQuality, int>
    {
        [VideoQuality.None] = 0,
        [VideoQuality.Unknown] = -1,
        [VideoQuality.SubSD_144p] = 1,
        [VideoQuality.SubSD_CIF] = 2,
        [VideoQuality.nHD] = 3,
        [VideoQuality.SD] = 4,
        [VideoQuality.DVD] = 5,
        [VideoQuality.HD] = 6,
        [VideoQuality.FullHD] = 7,
        [VideoQuality.QHD] = 8,
        [VideoQuality.UHD_4K] = 9,
        [VideoQuality.UHD_8K] = 10,
    };

    private static readonly IReadOnlyDictionary<int, VideoQuality> _idVideoQualities = _videoQualityIds.ToDictionary(
        x => x.Value,
        x => x.Key
    );

    public static int ToId(this VideoQuality quality)
    {
        if (_videoQualityIds.TryGetValue(quality, out var id))
            return id;

        throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
    }

    public static VideoQuality ToVideoQuality(this int id)
    {
        if (_idVideoQualities.TryGetValue(id, out var quality))
            return quality;

        if (Enum.IsDefined((VideoQuality)id))
            return (VideoQuality)id;

        throw new ArgumentOutOfRangeException(nameof(id), id, null);
    }

    #endregion
}

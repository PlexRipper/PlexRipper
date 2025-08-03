using Application.Contracts;

namespace PlexRipper.Application;

public static class DownloadPreviewMapper
{
    #region ToDTO

    private static int _currentId = 1;
    private const int NoMediaDataId = -1;

    public static DownloadPreviewDTO ToDTO(this DownloadPreview source) =>
        new()
        {
            // Media Id's can overlap, so we use a static counter to generate unique keys
            Key = Interlocked.Increment(ref _currentId).ToString(),
            Title = source.Title,
            Size = source.Size,
            Type = source.MediaType,
            Children = source.Children.ConvertAll(ToDTO),
            Qualities = source
                .Qualities.Select(x => new PlexMediaQualityDTO
                {
                    Quality = x.Quality,
                    MediaDataType = x.MediaDataType,
                    DataId = x.DataId,
                    MediaId = x.MediaId,
                })
                .ToList(),
        };

    public static DownloadPreviewContainerDTO ToDTO(this List<DownloadPreview> source)
    {
        var previews = source.ConvertAll(ToDTO);

        Dictionary<string, bool> FlattenKeysToDictionary(List<DownloadPreviewDTO> list)
        {
            var result = new Dictionary<string, bool>();

            void Traverse(DownloadPreviewDTO item, int depth)
            {
                result.Add(item.Key, true);

                // Don't need all the episode keys as well to be fully expanded
                if (depth >= 0)
                    return;

                foreach (var child in item.Children)
                    Traverse(child, depth + 1);
            }

            foreach (var item in list)
                Traverse(item, 0);

            return result;
        }

        return new DownloadPreviewContainerDTO
        {
            TotalSize = previews.Sum(x => x.Size),
            Expanded = FlattenKeysToDictionary(previews),
            Previews = previews,
        };
    }

    #endregion

    #region PlexMovie

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaDataList.Sum(x => x.Parts.Sum(p => p.Size)),
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = default,
            SeasonId = default,
            Children = [],
            Qualities = source
                .MediaDataList.SortByQuality()
                .Select(x => new PlexMediaQuality
                {
                    Quality = x.Quality,
                    MediaDataType = x.Type,
                    DataId = x.Id,
                    MediaId = source.Id,
                })
                .ToList(),
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexMovie> source) =>
        source.Select(x => ProjectToDownloadPreviewMapper(x));

    #endregion

    #region PlexTvShow

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = default,
            SeasonId = default,
            Children = [],
            Qualities = source
                .Qualities.SortByQuality()
                .Select(x => new PlexMediaQuality
                {
                    Quality = x.Quality,
                    MediaDataType = x.Type,
                    DataId = NoMediaDataId,
                    MediaId = source.Id,
                })
                .ToList(),
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexTvShow> source) =>
        source.Select(x => ProjectToDownloadPreviewMapper(x));

    #endregion

    #region PlexSeason

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShowSeason source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = source.TvShowId,
            SeasonId = default,
            Children = [],
            Qualities = source
                .Qualities.SortByQuality()
                .Select(x => new PlexMediaQuality
                {
                    Quality = x.Quality,
                    MediaDataType = x.Type,
                    DataId = NoMediaDataId,
                    MediaId = source.Id,
                })
                .ToList(),
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexTvShowSeason> source) =>
        source.Select(x => ProjectToDownloadPreviewMapper(x));

    #endregion

    #region PlexTvShowEpisode

    private static DownloadPreview ProjectToDownloadPreviewMapper(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            Size = source.MediaSize,
            ChildCount = source.ChildCount,
            MediaType = source.Type,
            TvShowId = source.TvShowId,
            SeasonId = source.TvShowSeasonId,
            Children = [],
            Qualities = source
                .MediaDataList.SortByQuality()
                .Select(x => new PlexMediaQuality
                {
                    Quality = x.Quality,
                    MediaDataType = x.Type,
                    MediaId = source.Id,
                    DataId = x.Id,
                })
                .ToList(),
        };

    public static IQueryable<DownloadPreview> ProjectToDownloadPreview(this IQueryable<PlexTvShowEpisode> source) =>
        source.Select(x => ProjectToDownloadPreviewMapper(x));

    private static TvShowEpisodeKey ProjectToEpisodeKey(this PlexTvShowEpisode source) =>
        new()
        {
            TvShowId = source.TvShowId,
            SeasonId = source.TvShowSeasonId,
            EpisodeId = source.Id,
            MediaDataList = source.MediaDataList,
        };

    public static IQueryable<TvShowEpisodeKey> ProjectToEpisodeKey(this IQueryable<PlexTvShowEpisode> source) =>
        source.Select(x => ProjectToEpisodeKey(x));

    #endregion
}

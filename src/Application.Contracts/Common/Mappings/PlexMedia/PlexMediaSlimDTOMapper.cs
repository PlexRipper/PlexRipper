using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaSlimDTOMapper
{
    #region PlexMovie

    public static PlexMediaSlimDTO ToSlimDTO(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SortIndex = source.SortIndex,
            Year = source.Year,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            GrandChildCount = 0,
            Qualities = source
                .MediaDataList.SortByQuality()
                .Select(x => new PlexMediaQualityDTO
                {
                    Quality = x.Quality,
                    MediaDataType = x.Type,
                    DataId = x.Id,
                    MediaId = source.Id,
                })
                .ToList(),
            Key = source.Key,
            MetaDataKey = source.MetaDataKey,
            PlexToken = string.Empty,
        };

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexMovie> source) =>
        source.Select(x => ToSlimDTO(x));

    #endregion

    #region PlexTvShow

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexTvShow> source) =>
        source.Select(x => ToSlimDTOMapper(x));

    public static PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            SortIndex = source.SortIndex,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            GrandChildCount = source.GrandChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            Key = source.Key,
            MetaDataKey = source.MetaDataKey,
            HasThumb = source.HasThumb,
            Qualities = source.Qualities.ToDTO(),
            PlexToken = string.Empty,
        };

    #endregion

    #region PlexEpisode

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SearchTitle = source.SearchTitle,
            SortIndex = source.SortIndex,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            GrandChildCount = 0,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            Key = source.Key,
            MetaDataKey = source.MetaDataKey,
            PlexToken = string.Empty,
            Qualities = source.MediaDataList.ToPlexMediaQuality(),
        };

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexTvShowEpisode> source) =>
        source.Select(x => ToSlimDTO(x));

    #endregion

    public static PlexMediaStatisticsDTO ToStatisticsDTO(this List<PlexMediaSlimDTO> source)
    {
        var stats = new PlexMediaStatisticsDTO
        {
            MovieCount = 0,
            TvShowCount = 0,
            SeasonCount = 0,
            EpisodeCount = 0,
            MediaSize = 0,
            MediaCount = 0,
            MediaList = source,
        };

        if (!source.Any())
        {
            return stats;
        }

        foreach (var entity in source)
        {
            stats.MediaCount++;
            stats.MediaSize += entity.MediaSize;
            switch (entity.Type)
            {
                case PlexMediaType.Movie:
                    stats.MovieCount++;
                    break;
                case PlexMediaType.TvShow:
                    stats.TvShowCount++;
                    stats.SeasonCount += entity.ChildCount;
                    stats.EpisodeCount += entity.GrandChildCount;
                    break;
            }
        }

        return stats;
    }
}

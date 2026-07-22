namespace Reaparr.Application;

public static partial class PlexMediaDTOMapper
{
    public static PlexMediaDTO ToDTO(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.TvShowSeasonId,
            MediaData = source.MediaDataList.ToDTO(),
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
            Qualities = source.MediaDataList.ToPlexMediaQuality(),
            ComparisonState = source.ComparisonState,
            PlexApiRatingKey = source.PlexApiRatingKey,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = [],
            PlexApiMetaDataKey = source.PlexApiMetaDataKey,
        };

    public static List<PlexMediaDataDTO> ToDTO(this ICollection<PlexTvShowEpisodeMediaData> source) =>
        source.Select(x => x.ToDTO()).ToList();

    public static PlexMediaDataDTO ToDTO(this PlexTvShowEpisodeMediaData source) =>
        new()
        {
            Duration = source.Duration,
            VideoResolution = source.VideoResolution,
            VideoCodec = source.VideoCodec,
            AudioCodec = source.AudioCodec,
        };
}

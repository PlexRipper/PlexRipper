using Application.Contracts;

namespace PlexRipper.Application;

public static partial class PlexMediaDTOMapper
{
    public static PlexMediaDTO ToDTO(this PlexTvShow plexTvShow, string plexToken)
    {
        var dto = plexTvShow.ToDTOMapper(plexToken);
        dto.Children = [];

        foreach (var tvShowSeason in plexTvShow.Seasons)
            dto.Children.Add(tvShowSeason.ToDTO(plexToken));

        return dto;
    }

    private static PlexMediaDTO ToDTOMapper(this PlexTvShow source, string plexToken) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.Id,
            TvShowSeasonId = default,
            MediaData = [],
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
            HasThumb = source.HasThumb,
            Qualities = source.TvShowMediaQualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Seasons.Select(x => x.ToDTO(plexToken)).ToList(),
            MetaDataKey = source.MetaDataKey,
            PlexToken = plexToken,
        };
}

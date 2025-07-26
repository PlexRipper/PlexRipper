using Application.Contracts;

namespace PlexRipper.Application;

public static partial class PlexMediaDTOMapper
{
    public static PlexMediaDTO ToDTO(this PlexTvShowSeason plexTvShowSeason, string plexToken)
    {
        var dto = plexTvShowSeason.ToDTOMapper(plexToken);
        dto.Children = [];

        foreach (var episode in plexTvShowSeason.Episodes)
            dto.Children.Add(episode.ToDTO(plexToken));

        return dto;
    }

    private static PlexMediaDTO ToDTOMapper(this PlexTvShowSeason source, string plexToken) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.Id,
            MediaData = [],
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
            Qualities = source.PlexTvShowSeasonMediaQualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Episodes.Select(x => x.ToDTO(plexToken)).ToList(),
            MetaDataKey = source.MetaDataKey,
            PlexToken = plexToken,
        };
}

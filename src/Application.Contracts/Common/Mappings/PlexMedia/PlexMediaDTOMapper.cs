using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaDTOMapper
{
    #region PlexMovie

    public static PlexMediaDTO ToDTO(this PlexMovie source, string plexToken) =>
        new()
        {
            Id = source.Id,
            TvShowId = default,
            TvShowSeasonId = default,
            MediaData = source.MetaDataList.ToDTO(),
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
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = [],
            MetaDataKey = source.MetaDataKey,
            PlexToken = plexToken,
        };

    #endregion

    #region PlexTvShow

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
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Seasons.ConvertAll(x => x.ToDTO(plexToken)),
            MetaDataKey = source.MetaDataKey,
            PlexToken = plexToken,
        };

    #endregion

    #region PlexSeason

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
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Episodes.ConvertAll(x => x.ToDTO(plexToken)),
            MetaDataKey = source.MetaDataKey,
            PlexToken = plexToken,
        };

    #endregion

    #region PlexEpisode

    public static PlexMediaDTO ToDTO(this PlexTvShowEpisode source, string plexToken) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.TvShowSeasonId,
            MediaData = source.MetaDataList.ToDTO(),
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
            Qualities = source.Qualities.ToDTO(),
            Key = source.Key,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = [],
            MetaDataKey = source.MetaDataKey,
            PlexToken = plexToken,
        };

    #endregion
}

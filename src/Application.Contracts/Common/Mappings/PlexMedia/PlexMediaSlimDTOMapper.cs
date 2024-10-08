using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaSlimDTOMapper
{
    public static PlexMediaSlimDTO ToSlimDTO(this PlexMediaSlim source) =>
        new()
        {
            Id = source.Id,
            Index = 0,
            Title = source.Title,
            SortTitle = source.SortTitle,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            FullThumbUrl = source.FullThumbUrl,
            Qualities = source.Qualities.ToDTO(),
            Children = [],
        };

    #region PlexMovie

    public static PlexMediaSlimDTO ToSlimDTO(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SortTitle = source.SortTitle,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            FullThumbUrl = source.FullThumbUrl,
            Qualities = source.Qualities.ToDTO(),
            Children = [],
        };

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexMovie> source) =>
        source.Select(x => ToSlimDTO(x));

    #endregion

    #region PlexTvShow

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShow plexTvShow)
    {
        var dto = plexTvShow.ToSlimDTOMapper();
        dto.Children = [];

        foreach (var tvShowSeason in plexTvShow.Seasons)
            dto.Children.Add(tvShowSeason.ToSlimDTO());

        return dto;
    }

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexTvShow> source) =>
        source.Select(x => ToSlimDTOMapper(x));

    private static PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SortTitle = source.SortTitle,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            FullThumbUrl = source.FullThumbUrl,
            Qualities = source.Qualities.ToDTO(),
            Children = source.Seasons.ConvertAll(ToSlimDTO),
        };

    #endregion

    #region PlexSeason

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShowSeason source)
    {
        var dto = source.ToSlimDTOMapper();
        dto.Children = [];

        foreach (var episode in source.Episodes)
            dto.Children.Add(episode.ToSlimDTO());

        return dto;
    }

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexTvShowSeason> source) =>
        source.Select(x => ToSlimDTO(x));

    private static PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShowSeason source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SortTitle = source.SortTitle,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            FullThumbUrl = source.FullThumbUrl,
            Qualities = source.Qualities.ToDTO(),
            Children = source.Episodes.ConvertAll(ToSlimDTO),
        };

    #endregion

    #region PlexEpisode

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            Title = source.Title,
            SortTitle = source.SortTitle,
            Year = source.Year,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            Type = source.Type,
            HasThumb = source.HasThumb,
            FullThumbUrl = source.FullThumbUrl,
            Qualities = source.Qualities.ToDTO(),
            Children = new List<PlexMediaSlimDTO>(),
        };

    public static IQueryable<PlexMediaSlimDTO> ProjectToMediaSlimDTO(this IQueryable<PlexTvShowEpisode> source) =>
        source.Select(x => ToSlimDTO(x));

    #endregion
}

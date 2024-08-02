using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaSlimDTOMapper
{
    #region PlexMovie

    public static PlexMediaSlimDTO ToSlimDTO(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            Index = source.Index,
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

    #endregion

    #region PlexTvShow

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShow plexTvShow)
    {
        var dto = plexTvShow.ToSlimDTOMapper();
        dto.Children = new List<PlexMediaSlimDTO>();
        if (plexTvShow.Seasons is not null)
        {
            foreach (var tvShowSeason in plexTvShow.Seasons)
                dto.Children.Add(tvShowSeason.ToSlimDTO());
        }

        return dto;
    }

    private static PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            Index = source.Index,
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
            Children = source.Seasons?.ConvertAll(ToSlimDTO) ?? new List<PlexMediaSlimDTO>(),
        };

    #endregion

    #region PlexSeason

    public static PlexMediaSlimDTO ToSlimDTO(this PlexTvShowSeason source)
    {
        var dto = source.ToSlimDTOMapper();
        dto.Children = new List<PlexMediaSlimDTO>();
        if (source.Episodes is not null)
        {
            foreach (var episode in source.Episodes)
                dto.Children.Add(episode.ToSlimDTO());
        }

        return dto;
    }

    private static PlexMediaSlimDTO ToSlimDTOMapper(this PlexTvShowSeason source) =>
        new()
        {
            Id = source.Id,

            Index = source.Index,
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
            Index = source.Index,
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

    #endregion
}

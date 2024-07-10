using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexMediaDTOMapper
{
    #region MediaData

    public static PlexMediaDataDTO ToDTO(this PlexMediaData source) =>
        new()
        {
            MediaFormat = source.MediaFormat,
            Duration = source.Duration,
            VideoResolution = source.VideoResolution,
            Width = source.Width,
            Height = source.Height,
            Bitrate = source.Bitrate,
            VideoCodec = source.VideoCodec,
            VideoFrameRate = source.VideoFrameRate,
            AspectRatio = source.AspectRatio,
            VideoProfile = source.VideoProfile,
            AudioProfile = source.AudioProfile,
            AudioCodec = source.AudioCodec,
            AudioChannels = source.AudioChannels,
            Parts = source.Parts.ConvertAll(ToDTO),
        };

    public static PlexMediaQualityDTO ToDTO(this PlexMediaQuality source) =>
        new()
        {
            Quality = source.Quality,
            DisplayQuality = source.DisplayQuality,
            HashId = source.HashId,
        };

    public static PlexMediaDataPartDTO ToDTO(this PlexMediaDataPart source) =>
        new()
        {
            ObfuscatedFilePath = source.ObfuscatedFilePath,
            Duration = source.Duration,
            File = source.File,
            Size = source.Size,
            Container = source.Container,
            VideoProfile = source.VideoProfile,
        };

    #endregion

    #region PlexMovie

    public static PlexMediaDTO ToDTO(this PlexMovie source) =>
        new()
        {
            Id = source.Id,
            TvShowId = default,
            TvShowSeasonId = default,
            MediaData = source.MovieData.ConvertAll(ToDTO),
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
            Qualities = source.Qualities.ConvertAll(ToDTO),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = new List<PlexMediaDTO>(),
        };

    #endregion

    #region PlexTvShow

    public static PlexMediaDTO ToDTO(this PlexTvShow plexTvShow)
    {
        var dto = plexTvShow.ToDTOMapper();
        dto.Children = new List<PlexMediaDTO>();
        if (plexTvShow.Seasons is not null)
        {
            foreach (var tvShowSeason in plexTvShow.Seasons)
                dto.Children.Add(tvShowSeason.ToDTO());
        }

        return dto;
    }

    private static PlexMediaDTO ToDTOMapper(this PlexTvShow source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.Id,
            TvShowSeasonId = default,
            MediaData = new List<PlexMediaDataDTO>(),
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
            Qualities = source.Qualities.ConvertAll(ToDTO),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Seasons.ConvertAll(ToDTO),
        };

    #endregion

    #region PlexSeason

    public static PlexMediaDTO ToDTO(this PlexTvShowSeason plexTvShowSeason)
    {
        var dto = plexTvShowSeason.ToDTOMapper();
        dto.Children = new List<PlexMediaDTO>();
        if (plexTvShowSeason.Episodes is not null)
        {
            foreach (var episode in plexTvShowSeason.Episodes)
                dto.Children.Add(episode.ToDTO());
        }

        return dto;
    }

    private static PlexMediaDTO ToDTOMapper(this PlexTvShowSeason source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.Id,
            MediaData = new List<PlexMediaDataDTO>(),
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
            Qualities = source.Qualities.ConvertAll(ToDTO),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = source.Episodes.ConvertAll(ToDTO),
        };

    #endregion

    #region PlexEpisode

    public static PlexMediaDTO ToDTO(this PlexTvShowEpisode source) =>
        new()
        {
            Id = source.Id,
            TvShowId = source.TvShowId,
            TvShowSeasonId = source.TvShowSeasonId,
            MediaData = source.EpisodeData.ConvertAll(ToDTO),
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
            Qualities = source.Qualities.ConvertAll(ToDTO),
            Key = source.Key,
            HasArt = source.HasArt,
            HasBanner = source.HasBanner,
            HasTheme = source.HasTheme,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            Children = new List<PlexMediaDTO>(),
        };

    #endregion
}

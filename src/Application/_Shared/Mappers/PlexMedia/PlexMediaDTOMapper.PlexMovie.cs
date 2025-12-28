using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public static partial class PlexMediaDTOMapper
{
    public static PlexMediaDTO ToDTO(this PlexMovie source, string plexToken) =>
        new()
        {
            Id = source.Id,
            TvShowId = default,
            TvShowSeasonId = default,
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

    public static List<PlexMediaDataDTO> ToDTO(this ICollection<PlexMovieMediaData> source) =>
        source.Select(x => x.ToDTO()).ToList();

    public static PlexMediaDataDTO ToDTO(this PlexMovieMediaData source) =>
        new()
        {
            Duration = source.Duration,
            VideoResolution = source.RawVideoResolution,
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
            Parts = source.Parts.ToDTO(),
        };

    public static List<LibraryMediaItemPartDTO> ToDTO(this ICollection<PlexMovieMediaDataPart> source) =>
        source.Select(x => x.ToDTO()).ToList();

    public static LibraryMediaItemPartDTO ToDTO(this PlexMovieMediaDataPart source) =>
        new()
        {
            Id = source.PlexId,
            Key = source.Key,
            Duration = source.Duration,
            File = source.File,
            Size = source.Size,
            Container = source.Container,
            Stream = [],
        };
}

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
            Indexes = source.Indexes,
            Duration = source.Duration,
            File = source.File,
            Size = source.Size,
            Container = source.Container,
            VideoProfile = source.VideoProfile,
            AudioProfile = source.AudioProfile,
            Stream = source.Streams.ToDTO(),
        };

    public static List<LibraryMediaItemStreamDTO> ToDTO(this ICollection<PlexMovieMediaDataStream> source) =>
        source.Select(x => x.ToDTO()).ToList();

    public static LibraryMediaItemStreamDTO ToDTO(this PlexMovieMediaDataStream source) =>
        new()
        {
            Id = source.PlexId,
            StreamType = source.StreamType,
            Default = source.Default,
            Codec = source.Codec,
            Index = source.Index,
            Bitrate = source.Bitrate,
            Language = source.Language,
            LanguageTag = source.LanguageTag,
            LanguageCode = source.LanguageCode,
            DOVIBLCompatID = source.DOVIBLCompatID,
            DOVIBLPresent = source.DOVIBLPresent,
            DOVIELPresent = source.DOVIELPresent,
            DOVILevel = source.DOVILevel,
            DOVIPresent = source.DOVIPresent,
            DOVIProfile = source.DOVIProfile,
            DOVIRPUPresent = source.DOVIRPUPresent,
            DOVIVersion = source.DOVIVersion,
            BitDepth = source.BitDepth,
            ChromaLocation = source.ChromaLocation,
            ChromaSubsampling = source.ChromaSubsampling,
            CodedHeight = source.CodedHeight,
            CodedWidth = source.CodedWidth,
            ColorPrimaries = source.ColorPrimaries,
            ColorRange = source.ColorRange,
            ColorSpace = source.ColorSpace,
            ColorTrc = source.ColorTrc,
            FrameRate = source.FrameRate,
            Height = source.Height,
            Level = source.Level,
            Original = source.Original,
            HasScalingMatrix = source.HasScalingMatrix,
            Profile = source.Profile,
            ScanType = source.ScanType,
            RefFrames = source.RefFrames,
            Width = source.Width,
            DisplayTitle = source.DisplayTitle,
            ExtendedDisplayTitle = source.ExtendedDisplayTitle,
            Selected = source.Selected,
            Forced = source.Forced,
            Channels = source.Channels,
            AudioChannelLayout = source.AudioChannelLayout,
            SamplingRate = source.SamplingRate,
            CanAutoSync = source.CanAutoSync,
            HearingImpaired = source.HearingImpaired,
            Dub = source.Dub,
            Title = source.Title,
        };
}

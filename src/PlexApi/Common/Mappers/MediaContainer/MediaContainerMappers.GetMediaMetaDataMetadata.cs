using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public static partial class MediaContainerMappers
{
    public static LibraryMediaItemDTO ToMediaItemDTO(this GetMediaMetaDataMetadata data)
    {
        return new LibraryMediaItemDTO
        {
            RatingKey = data.RatingKey,
            Key = data.Key,
            Type = data.Type.ToPlexMediaTypeFromPlexApi(),
            Title = data.Title,
            Summary = data.Summary,
            Year = data.Year,
            TitleSort = data.Title.ToSortTitle(),
            OriginalTitle = data.OriginalTitle ?? string.Empty,
            ChildCount = data.ChildCount ?? 0,
            Media = data.Media?.Select(x => x.ToItemMediaDTO()).ToList() ?? [],
            Genre = data.Genre?.Select(x => x.ToDTO()).ToList() ?? [],
            Country = data.Country?.Select(x => x.ToDTO()).ToList() ?? [],
            Role = data.Role?.Select(x => x.ToDTO()).ToList() ?? [],
            Studio = data.Studio ?? string.Empty,
            ContentRating = data.ContentRating ?? string.Empty,

            // Duration is in milliseconds and we want seconds
            Duration = data.Duration / 1000,
            Thumb = data.Thumb,
            Art = data.Art,
            Theme = data.Theme ?? string.Empty,
            Guid = data.Guid,
            AddedAt = DateTimeExtensions.FromUnixTime(data.AddedAt),
            UpdatedAt = DateTimeExtensions.FromUnixTime(data.UpdatedAt),
            OriginallyAvailableAt = data.OriginallyAvailableAt?.ToString() ?? string.Empty,
            Ratings = data.Ratings?.Select(x => x.ToDTO()).ToList() ?? [],
            Guids = data.Guids?.Select(x => new MetaDataGuidsDTO { Id = x.Id }).ToList() ?? [],
            GrandparentTitle = data.GrandparentTitle ?? string.Empty,
            ParentTitle = data.ParentTitle ?? string.Empty,
            ParentGuid = data.ParentGuid ?? string.Empty,
            ParentRatingKey = data.ParentRatingKey ?? string.Empty,
            AudienceRating = data.AudienceRating,
            Rating = data.Rating ?? 0,
        };
    }

    public static LibraryMediaItemGenreDTO ToDTO(this GetMediaMetaDataGenre x) =>
        new()
        {
            Id = x.Id,
            Filter = x.Filter,
            Tag = x.Tag,
        };

    public static MetaDataCountryDTO ToDTO(this GetMediaMetaDataCountry x) =>
        new()
        {
            Id = x.Id,
            Filter = x.Filter,
            Tag = x.Tag,
        };

    public static LibraryMediaItemRoleDTO ToDTO(this GetMediaMetaDataRole x) =>
        new()
        {
            Id = x.Id,
            Filter = x.Filter,
            Tag = x.Tag,
            TagKey = x.TagKey,
            Role = x.Role ?? string.Empty,
            Thumb = x.Thumb ?? string.Empty,
        };

    public static MetaDataRatingsDTO ToDTO(this Ratings x) =>
        new()
        {
            Image = x.Image,
            Type = x.Type,
            Value = x.Value,
        };

    public static LibraryMediaItemMediaDTO ToItemMediaDTO(this GetMediaMetaDataMedia media) =>
        new()
        {
            Id = media.Id,
            Duration = media.Duration,
            Bitrate = media.Bitrate,
            Width = media.Width,
            Height = media.Height,
            AspectRatio = media.AspectRatio,
            AudioChannels = media.AudioChannels,
            AudioCodec = media.AudioCodec,
            VideoCodec = media.VideoCodec,
            VideoResolution = media.VideoResolution,
            Container = media.Container,
            VideoFrameRate = media.VideoFrameRate,
            VideoProfile = media.VideoProfile,
            AudioProfile = media.AudioProfile ?? string.Empty,
            HasVoiceActivity = media.HasVoiceActivity,
            Parts = media.Part.Select(x => x.ToItemPartDTO()).ToList(),
        };

    public static LibraryMediaItemPartDTO ToItemPartDTO(this GetMediaMetaDataPart part) =>
        new()
        {
            Id = part.Id,
            Accessible = part.Accessible,
            Exists = part.Exists,
            Key = part.Key,
            Indexes = part.Indexes,
            Duration = part.Duration,
            File = part.File,
            Size = part.Size,
            Container = part.Container,
            VideoProfile = part.VideoProfile,
            AudioProfile = part.AudioProfile ?? string.Empty,
            Stream = part.Stream?.Select(x => x.ToItemStreamDTO()).ToList() ?? [],
        };

    public static LibraryMediaItemStreamDTO ToItemStreamDTO(this GetMediaMetaDataStream source) =>
        new()
        {
            Id = source.Id,
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

using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public static partial class MediaContainerMappers
{
    public static LibraryMediaItemDTO ToMediaItemDTO(this GetAllMediaLibraryMetadata data)
    {
        return new LibraryMediaItemDTO
        {
            RatingKey = data.RatingKey,
            Key = data.Key,
            Type = data.Type.ToString().ToPlexMediaTypeFromPlexApi(),
            Title = data.Title,
            Summary = data.Summary,
            Year = data.Year,
            TitleSort = data.Title.ToSortTitle(),
            OriginalTitle = data.OriginalTitle ?? string.Empty,
            ChildCount = data.ChildCount,
            Media = data.Media?.Select(x => x.ToItemMediaDTO()).ToList() ?? [],
            Genre = data.Genre?.Select(x => x.ToDTO()).ToList() ?? [],
            Country = data.Country?.Select(x => x.ToDTO()).ToList() ?? [],
            Role = data.Role?.Select(x => x.ToDTO()).ToList() ?? [],
            Studio = data.Studio,
            ContentRating = data.ContentRating,

            // Duration is in milliseconds and we want seconds
            Duration = data.Duration / 1000,
            Thumb = data.Thumb,
            Art = data.Art,
            Theme = data.Theme,
            Guid = data.Guid,
            AddedAt = DateTimeExtensions.FromUnixTime(data.AddedAt),
            UpdatedAt = DateTimeExtensions.FromUnixTime(data.UpdatedAt ?? 0),
            OriginallyAvailableAt = data.OriginallyAvailableAt.ToString(),
            Ratings = [],
            Guids = data.Guids?.Select(x => new MetaDataGuidsDTO { Id = x.Id }).ToList() ?? [],
            GrandparentTitle = data.GrandparentTitle ?? string.Empty,
            ParentTitle = data.ParentTitle ?? string.Empty,
            ParentGuid = data.ParentGuid ?? string.Empty,
            ParentRatingKey = data.ParentRatingKey ?? string.Empty,
            AudienceRating = data.AudienceRating,
            Rating = data.Rating,
        };
    }

    public static LibraryMediaItemGenreDTO ToDTO(this GetAllMediaLibraryGenre x) =>
        new()
        {
            Id = -1,
            Filter = string.Empty,
            Tag = x.Tag,
        };

    public static MetaDataCountryDTO ToDTO(this GetAllMediaLibraryCountry x) =>
        new()
        {
            Id = -1,
            Filter = string.Empty,
            Tag = x.Tag,
        };

    public static LibraryMediaItemRoleDTO ToDTO(this GetAllMediaLibraryRole x) =>
        new()
        {
            Id = -1,
            Filter = string.Empty,
            Tag = x.Tag,
            TagKey = string.Empty,
            Role = string.Empty,
            Thumb = string.Empty,
        };

    public static LibraryMediaItemMediaDTO ToItemMediaDTO(this GetAllMediaLibraryMedia media) =>
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

    public static LibraryMediaItemPartDTO ToItemPartDTO(this GetAllMediaLibraryPart part) =>
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

    public static LibraryMediaItemStreamDTO ToItemStreamDTO(this GetAllMediaLibraryStream source) =>
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

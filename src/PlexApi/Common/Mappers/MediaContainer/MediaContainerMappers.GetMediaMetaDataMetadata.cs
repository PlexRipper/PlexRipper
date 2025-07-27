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
            Type = data.Type.ToPlexMediaType(),
            Title = data.Title,
            Summary = data.Summary,
            Year = data.Year ?? 0,
            TitleSort = data.Title.ToSortTitle(),
            OriginalTitle = data.OriginalTitle ?? string.Empty,
            ChildCount = data.ChildCount,
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
            Theme = data.Theme,
            Guid = data.Guid,
            AddedAt = DateTimeExtensions.FromUnixTime(data.AddedAt),
            UpdatedAt = DateTimeExtensions.FromUnixTime(data.UpdatedAt ?? 0),
            OriginallyAvailableAt = data.OriginallyAvailableAt?.ToString() ?? string.Empty,
            Ratings = data.Ratings?.Select(x => x.ToDTO()).ToList() ?? [],
            Guids = data.Guids?.Select(x => new MetaDataGuidsDTO { Id = x.Id }).ToList() ?? [],
            GrandparentTitle = data.GrandparentTitle ?? string.Empty,
            ParentTitle = data.ParentTitle ?? string.Empty,
            ParentGuid = data.ParentGuid ?? string.Empty,
            ParentRatingKey = data.ParentRatingKey ?? string.Empty,
            AudienceRating = data.AudienceRating,
            Rating = (float)Math.Round(data.Rating, 2),
        };
    }

    public static LibraryMediaItemGenreDTO ToDTO(this GetMediaMetaDataGenre x) =>
        new()
        {
            Name = x.Tag,
            PlexId = x.Id,
            Filter = x.Filter,
            Key = x.Tag.ToMd5Hash(),
        };

    public static LibraryMediaItemCountryDTO ToDTO(this GetMediaMetaDataCountry x) =>
        new()
        {
            Name = x.Tag,
            PlexId = x.Id,
            Filter = x.Filter,
            Key = x.Tag.ToMd5Hash(),
        };

    public static LibraryMediaItemRoleDTO ToDTO(this GetMediaMetaDataRole x) =>
        new()
        {
            Name = x.Tag,
            PlexId = x.Id,
            Role = x.Role,
            Filter = x.Filter,
            Thumb = x.Thumb,
            Key = x.Tag.ToMd5Hash(),
        };

    public static MetaDataRatingsDTO ToDTO(this GetMediaMetaDataRatings x) =>
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
            Duration = media.Duration ?? 0,
            Bitrate = media.Bitrate ?? 0,
            Width = media.Width ?? 0,
            Height = media.Height ?? 0,
            AspectRatio = media.AspectRatio ?? 0,
            AudioChannels = media.AudioChannels ?? 0,
            AudioCodec = media.AudioCodec ?? string.Empty,
            VideoCodec = media.VideoCodec ?? string.Empty,
            VideoResolution =
                media.VideoResolution ?? media.Part?.FirstOrDefault()?.File.ParseQualityFromFileName() ?? string.Empty,
            Container = media.Container ?? string.Empty,
            VideoFrameRate = media.VideoFrameRate ?? string.Empty,
            VideoProfile = media.VideoProfile ?? string.Empty,
            AudioProfile = media.AudioProfile ?? string.Empty,
            HasVoiceActivity = media.HasVoiceActivity ?? false,
            Parts = media.Part?.Select(x => x.ToItemPartDTO()).ToList() ?? [],
        };

    public static LibraryMediaItemPartDTO ToItemPartDTO(this GetMediaMetaDataPart part) =>
        new()
        {
            Id = part.Id,
            Accessible = part.Accessible,
            Exists = part.Exists,
            Key = part.Key ?? string.Empty,
            Indexes = part.Indexes,
            Duration = part.Duration ?? 0,
            File = part.File ?? string.Empty,
            Size = part.Size ?? 0,
            Container = part.Container ?? string.Empty,
            VideoProfile = part.VideoProfile ?? string.Empty,
            AudioProfile = part.AudioProfile ?? string.Empty,
            Stream = part.Stream?.Select(x => x.ToItemStreamDTO()).ToList() ?? [],
        };

    public static LibraryMediaItemStreamDTO ToItemStreamDTO(this GetMediaMetaDataStream source) =>
        new()
        {
            Id = source.Id,
            StreamType = source.StreamType switch
            {
                1 => StreamType.Video,
                2 => StreamType.Audio,
                3 => StreamType.Subtitle,
                _ => StreamType.Unknown,
            },
            Default = source.Default,
            Codec = source.Codec ?? string.Empty,
            Index = source.Index,
            Bitrate = source.Bitrate ?? 0,
            Language = source.Language ?? string.Empty,
            LanguageTag = source.LanguageTag ?? string.Empty,
            LanguageCode = source.LanguageCode ?? string.Empty,
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
            DisplayTitle = source.DisplayTitle ?? string.Empty,
            ExtendedDisplayTitle = source.ExtendedDisplayTitle ?? string.Empty,
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

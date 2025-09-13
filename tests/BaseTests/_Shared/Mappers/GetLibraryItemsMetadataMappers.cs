using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace Reaparr.BaseTests;

public static class GetLibrarySectionsAllMetadataMappers
{
    public static GetLibrarySectionsAllMetadata ToLibraryItemsMetadata(this GetMediaMetaDataMetadata source)
    {
        return new GetLibrarySectionsAllMetadata
        {
            RatingKey = source.RatingKey,
            Key = source.Key,
            Guid = source.Guid,
            Studio = source.Studio,
            Type = (GetLibrarySectionsAllLibraryType)(int)source.Type,
            Title = source.Title,
            Slug = source.Slug,
            ContentRating = source.ContentRating,
            Summary = source.Summary,
            Rating = source.Rating,
            AudienceRating = source.AudienceRating,
            Year = source.Year,
            SeasonCount = source.SeasonCount,
            Tagline = source.Tagline,
            Thumb = source.Thumb,
            Art = source.Art,
            Duration = source.Duration,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            AudienceRatingImage = source.AudienceRatingImage,
            ChapterSource = source.ChapterSource,
            PrimaryExtraKey = source.PrimaryExtraKey,
            RatingImage = source.RatingImage,
            GrandparentRatingKey = source.GrandparentRatingKey,
            GrandparentGuid = source.GrandparentGuid,
            GrandparentKey = source.GrandparentKey,
            GrandparentTitle = source.GrandparentTitle,
            GrandparentThumb = source.GrandparentThumb,
            GrandparentSlug = source.GrandparentSlug,
            GrandparentArt = source.GrandparentArt,
            GrandparentTheme = source.GrandparentTheme,
            Media = source.Media?.Select(m => m.ToLibraryItemsMedia()).ToList(),
            Genre = source.Genre?.Select(g => new GetLibrarySectionsAllGenre { Tag = g.Tag }).ToList(),
            Country = source.Country?.Select(c => new GetLibrarySectionsAllCountry { Tag = c.Tag }).ToList(),
            Director = source.Director?.Select(_ => new GetLibrarySectionsAllDirector()).ToList(),
            Writer = source.Writer?.Select(_ => new GetLibrarySectionsAllWriter()).ToList(),
            Role = source.Role?.Select(r => r.ToLibraryItemsRole()).ToList(),
            UltraBlurColors = source.UltraBlurColors != null ? new GetLibrarySectionsAllUltraBlurColors() : null,
            Image = source.Image?.Select(_ => new GetLibrarySectionsAllImage()).ToList(),
            TitleSort = source.TitleSort,
            ViewCount = source.ViewCount,
            LastViewedAt = source.LastViewedAt,
            OriginalTitle = source.OriginalTitle,
            ViewOffset = source.ViewOffset,
            SkipCount = source.SkipCount,
            Index = source.Index,
            Theme = source.Theme,
            LeafCount = source.LeafCount,
            ViewedLeafCount = source.ViewedLeafCount,
            ChildCount = source.ChildCount,
            ParentRatingKey = source.ParentRatingKey,
            ParentGuid = source.ParentGuid,
            ParentKey = source.ParentKey,
            ParentTitle = source.ParentTitle,
            ParentIndex = source.ParentIndex,
            ParentThumb = source.ParentThumb,
        };
    }

    private static GetLibrarySectionsAllMedia ToLibraryItemsMedia(this GetMediaMetaDataMedia source) =>
        new()
        {
            Id = (int)source.Id,
            Duration = source.Duration,
            Bitrate = source.Bitrate,
            Width = source.Width,
            Height = source.Height,
            AspectRatio = source.AspectRatio,
            AudioChannels = source.AudioChannels,
            AudioCodec = source.AudioCodec,
            AudioProfile = source.AudioProfile,
            VideoCodec = source.VideoCodec,
            VideoResolution = source.VideoResolution,
            Container = source.Container ?? string.Empty,
            VideoFrameRate = source.VideoFrameRate,
            VideoProfile = source.VideoProfile,
            HasVoiceActivity = source.HasVoiceActivity,
            OptimizedForStreaming = GetLibrarySectionsAllOptimizedForStreaming.CreateBoolean(
                source.OptimizedForStreaming?.Boolean ?? false
            ),
            Has64bitOffsets = source.Has64bitOffsets,
            Part = source.Part?.Select(ToLibraryItemsPart).ToList() ?? [],
        };

    private static GetLibrarySectionsAllPart ToLibraryItemsPart(this GetMediaMetaDataPart source) =>
        new()
        {
            Id = (int)source.Id,
            Key = source.Key ?? string.Empty,
            Duration = source.Duration,
            File = source.File ?? string.Empty,
            Size = source.Size ?? 0,
            Container = source.Container ?? string.Empty,
            AudioProfile = source.AudioProfile ?? string.Empty,
            Has64bitOffsets = source.Has64bitOffsets,
            OptimizedForStreaming = GetLibrarySectionsAllLibraryOptimizedForStreaming.CreateBoolean(
                source.OptimizedForStreaming?.Boolean ?? false
            ),
            VideoProfile = source.VideoProfile ?? string.Empty,
            Indexes = source.Indexes,
            HasThumbnail = null,
        };

    private static GetLibrarySectionsAllRole ToLibraryItemsRole(this GetMediaMetaDataRole source) =>
        new() { Tag = source.Tag };
}

using LukeHagar.PlexAPI.SDK.Models.Requests;

namespace PlexRipper.BaseTests;

public static class GetLibraryItemsMetadataMappers
{
    public static GetLibraryItemsMetadata ToLibraryItemsMetadata(this GetMediaMetaDataMetadata source)
    {
        return new GetLibraryItemsMetadata
        {
            RatingKey = source.RatingKey,
            Key = source.Key,
            Guid = source.Guid,
            Studio = source.Studio,
            LibrarySectionID = source.LibrarySectionID,
            LibrarySectionTitle = source.LibrarySectionTitle,
            LibrarySectionKey = source.LibrarySectionKey,
            Type = (GetLibraryItemsType)(int)source.Type,
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
            Genre = source.Genre?.Select(g => new GetLibraryItemsGenre { Tag = g.Tag }).ToList(),
            Country = source.Country?.Select(c => new GetLibraryItemsCountry { Tag = c.Tag }).ToList(),
            Director = source.Director?.Select(_ => new GetLibraryItemsDirector()).ToList(),
            Writer = source.Writer?.Select(_ => new GetLibraryItemsWriter()).ToList(),
            Role = source.Role?.Select(r => r.ToLibraryItemsRole()).ToList(),
            Location = source.Location?.Select(l => new GetLibraryItemsLocation { Path = l.Path }).ToList(),
            UltraBlurColors = source.UltraBlurColors != null ? new GetLibraryItemsUltraBlurColors() : null,
            Image = source.Image?.Select(_ => new GetLibraryItemsImage()).ToList(),
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

    private static GetLibraryItemsMedia ToLibraryItemsMedia(this GetMediaMetaDataMedia source) =>
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
            OptimizedForStreaming = GetLibraryItemsOptimizedForStreaming.CreateBoolean(
                source.OptimizedForStreaming?.Boolean ?? false
            ),
            Has64bitOffsets = source.Has64bitOffsets,
            Part = source.Part?.Select(ToLibraryItemsPart).ToList() ?? [],
        };

    private static GetLibraryItemsPart ToLibraryItemsPart(this GetMediaMetaDataPart source) =>
        new()
        {
            Id = (int)source.Id,
            Key = source.Key ?? string.Empty,
            Duration = source.Duration,
            File = source.File ?? string.Empty,
            Size = source.Size,
            Container = source.Container ?? string.Empty,
            AudioProfile = source.AudioProfile ?? string.Empty,
            Has64bitOffsets = source.Has64bitOffsets,
            OptimizedForStreaming = GetLibraryItemsLibraryOptimizedForStreaming.CreateBoolean(
                source.OptimizedForStreaming?.Boolean ?? false
            ),
            VideoProfile = source.VideoProfile ?? string.Empty,
            Indexes = source.Indexes,

            // set to null to avoid Unable to cast object of type 'System.Int64' to type 'System.String'.
            HasThumbnail = null,
        };

    private static GetLibraryItemsRole ToLibraryItemsRole(this GetMediaMetaDataRole source) =>
        new()
        {
            Id = source.Id,
            Thumb = source.Thumb,
            Tag = source.Tag,
            Role = source.Role,
        };
}

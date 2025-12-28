namespace Reaparr.PlexApi;

public static partial class PlexMediaDataMapper
{
    public static List<PlexTvShowEpisode> ToPlexTvShowEpisodes(this List<LibraryMediaItemDTO> source) =>
        source.Select(value => value.ToPlexTvShowEpisode()).ToList();

    public static PlexTvShowEpisode ToPlexTvShowEpisode(this LibraryMediaItemDTO source) =>
        new()
        {
            Id = 0,
            Title = source.Title,
            FullTitle = $"{source.GrandparentTitle}/{source.ParentTitle}/{source.Title}",
            Year = source.Year,

            // This is set later on
            SortIndex = 0,
            EpisodeNumber = source.Index,

            SearchTitle = source.Title.ToSearchTitle(),
            Guid = source.Guid,
            ParentGuid = source.ParentGuid,

            Guid_IMDB = source.Guids.GetImdbId(),
            Guid_TMDB = source.Guids.GetTmdbId(),
            Guid_TVDB = source.Guids.GetTvdbId(),

            Duration = source.Duration,
            MediaSize = source.Media.Sum(y => y.Parts.Sum(z => z.Size)),
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            MediaDataList = source.Media.ToEpisodeMediaDataList(source),
            ParentKey = source.GetParentKey(),

            Type = PlexMediaType.None,
            Key = source.RatingKey,
            MetaDataKey = RetrieveMetaDataKey(source),
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt.ToDateTime(),
            HasThumb = !string.IsNullOrEmpty(source.Thumb),
            HasArt = !string.IsNullOrEmpty(source.Art),
            HasTheme = !string.IsNullOrEmpty(source.Theme),

            // Ignore the following
            PlexLibrary = default,
            PlexServer = default,
            PlexLibraryId = default,
            PlexServerId = default,
            FullBannerUrl = string.Empty,
        };

    public static ICollection<PlexTvShowEpisodeMediaData> ToEpisodeMediaDataList(
        this List<LibraryMediaItemMediaDTO> source,
        LibraryMediaItemDTO root
    ) => source.Select(x => x.ToEpisodeMediaDataList(root)).ToList();

    public static PlexTvShowEpisodeMediaData ToEpisodeMediaDataList(
        this LibraryMediaItemMediaDTO source,
        LibraryMediaItemDTO root
    ) =>
        new()
        {
            Id = 0,
            PlexId = source.Id,
            Duration = source.Duration,
            Bitrate = source.Bitrate,
            Width = source.Width,
            Height = source.Height,
            AspectRatio = source.AspectRatio,
            AudioChannels = source.AudioChannels,
            AudioCodec = source.AudioCodec,
            VideoCodec = source.VideoCodec,
            RawVideoResolution = source.VideoResolution,
            Quality = source.VideoResolution.ToVideoQuality(),
            Container = source.Container,
            VideoFrameRate = source.VideoFrameRate,
            VideoProfile = source.VideoProfile,
            AudioProfile = source.AudioProfile,
            HasVoiceActivity = source.HasVoiceActivity,
            Parts = source.Parts.ToPlexTvShowEpisodeModel(root),

            // Ignore the following
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexTvShowEpisodeId = 0,
        };

    public static ICollection<PlexTvShowEpisodeMediaDataPart> ToPlexTvShowEpisodeModel(
        this List<LibraryMediaItemPartDTO> source,
        LibraryMediaItemDTO root
    ) => source.Select(x => x.ToPlexTvShowEpisodeModel(root)).ToList();

    public static PlexTvShowEpisodeMediaDataPart ToPlexTvShowEpisodeModel(
        this LibraryMediaItemPartDTO source,
        LibraryMediaItemDTO root
    ) =>
        new()
        {
            Id = 0,
            PlexId = source.Id,
            Key = source.Key,
            Duration = source.Duration,
            File = source.File,
            Size = source.Size,
            RatingKey = root.RatingKey,
            Container = source.Container,

            // Ignore the following
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexTvShowEpisodeId = 0,
            PlexTvShowEpisodeMediaDataId = 0,
            LastSyncedAt = default,
            VideoCodec = string.Empty,
            FrameRate = 0,
            Resolution = string.Empty,
            Source = ReleaseSource.None,
            HasMetadata = false,
            PrimaryAudioCodec = string.Empty,
            AudioChannels = string.Empty,
            OriginalFilename = string.Empty,
            GeneratedFilename = string.Empty,
        };
}

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
    ) => source.SelectMany(x => x.ToEpisodeMediaDataList(root)).ToList();

    public static ICollection<PlexTvShowEpisodeMediaData> ToEpisodeMediaDataList(
        this LibraryMediaItemMediaDTO source,
        LibraryMediaItemDTO root
    ) => source.Parts.Select(part => part.ToPlexTvShowEpisodeModel(source, root)).ToList();

    public static PlexTvShowEpisodeMediaData ToPlexTvShowEpisodeModel(
        this LibraryMediaItemPartDTO source,
        LibraryMediaItemMediaDTO mediaItem,
        LibraryMediaItemDTO root
    ) =>
        new()
        {
            Id = 0,
            PlexMediaId = mediaItem.Id,
            PlexPartId = source.Id,
            Key = source.Key,
            Duration = source.Duration,
            OriginalFilename = source.File.GetFileName(),
            Size = source.Size,
            Container = source.Container,
            RatingKey = root.RatingKey,
            Quality = mediaItem.VideoResolution.ToVideoQuality(),
            FrameRate = mediaItem.VideoFrameRate,
            VideoCodec = mediaItem.VideoCodec,
            VideoResolution = mediaItem.VideoResolution,
            AudioCodec = mediaItem.AudioCodec,
            AudioChannels = mediaItem.AudioChannels,

            // Ignore the following
            PlexLibraryId = 0,
            PlexServerId = 0,
            PlexTvShowEpisodeId = 0,
            LastSyncedAt = default,
            Source = ReleaseSource.None,
            HasMetadata = false,
            GeneratedFilename = string.Empty,
        };
}

using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Metadata = LukeHagar.PlexAPI.SDK.Models.Components.Metadata;

namespace Reaparr.PlexApi;

internal static class PlexLibraryMediaApi
{
    public static async Task<Result<int>> GetLibraryMediaTotalCountAsync(
        this IPlexAPI client,
        long libraryKey,
        PlexMediaType mediaType,
        CancellationToken cancellationToken
    )
    {
        var response = await client
            .Content.ListContentAsync(
                CreateListContentRequest(libraryKey, mediaType, 0, 0, includeStreams: false, includeMetadata: false)
            )
            .ToResponse(cancellationToken);
        if (response.IsFailed)
            return response.ToResult().LogIfFailed();

        var rawValue = response.Value.MediaContainerWithMetadata?.MediaContainer?.TotalSize ?? 0;
        var safeValue = (int)Math.Max(0, Math.Min(rawValue, int.MaxValue));
        return Result.Ok(safeValue);
    }

    public static async Task<Result<PlexLibraryMediaPage>> GetMetadataForLibraryAsync(
        this IPlexAPI client,
        long libraryKey,
        PlexMediaType mediaType,
        int startIndex,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        var response = await client
            .Content.ListContentAsync(
                CreateListContentRequest(libraryKey, mediaType, startIndex, pageSize, includeStreams: true)
            )
            .ToResponse(cancellationToken);
        if (response.IsFailed)
            return response.ToResult().LogIfFailed();

        var mediaContainer = response.Value.MediaContainerWithMetadata?.MediaContainer;
        var metadata = mediaContainer?.Metadata ?? [];
        int? total = mediaContainer?.TotalSize is { } rawTotal
            ? (int)Math.Max(0, Math.Min(rawTotal, int.MaxValue))
            : null;
        return Result.Ok(new PlexLibraryMediaPage(metadata.Select(x => x.ToMediaItemDTO()).ToList(), total));
    }

    public static Task<Result<List<PlexLibraryMediaItem>>> GetPhotoLibraryItemsAsync(
        this IPlexAPI client,
        long libraryKey,
        PlexMediaType mediaType,
        CancellationToken cancellationToken
    ) =>
        ReadPhotoPagesAsync(
            offset =>
                client.Content.ListContentAsync(
                        CreateListContentRequest(
                            libraryKey,
                            mediaType,
                            offset,
                            100,
                            includeStreams: false,
                            requestMediaTypeOverride: mediaType == PlexMediaType.OtherVideos ? MediaType.Clip : null
                        )
                    )
                    .ToResponse(cancellationToken),
            response => response.MediaContainerWithMetadata?.MediaContainer,
            mediaType,
            "photo media"
        );

    public static Task<Result<List<PlexLibraryMediaItem>>> GetPhotoFolderItemsAsync(
        this IPlexAPI client,
        long libraryKey,
        string? parent,
        CancellationToken cancellationToken
    ) =>
        ReadPhotoPagesAsync(
            offset =>
                client.Library.GetSectionFolderAsync(
                        new GetSectionFolderRequest
                        {
                            SectionId = libraryKey,
                            Parent = parent,
                            XPlexContainerStart = offset,
                            XPlexContainerSize = 100,
                            IncludeMeta = BoolInt.True,
                        }
                    )
                    .ToResponse(cancellationToken),
            response => response.MediaContainerWithMetadata?.MediaContainer,
            PlexMediaType.PhotoImage,
            "photo folder"
        );

    public static Task<Result<List<PlexLibraryMediaItem>>> GetPhotoAlbumChildrenAsync(
        this IPlexAPI client,
        int albumKey,
        CancellationToken cancellationToken
    ) =>
        ReadPhotoPagesAsync(
            offset =>
                client.Library.GetMetadataChildrenAsync(
                        new GetMetadataChildrenRequest
                        {
                            Id = albumKey,
                            XPlexContainerStart = offset,
                            XPlexContainerSize = 100,
                            IncludeMeta = BoolInt.True,
                        }
                    )
                    .ToResponse(cancellationToken),
            response => response.MediaContainerWithMetadata?.MediaContainer,
            PlexMediaType.PhotoImage,
            "photo album children"
        );

    private static ListContentRequest CreateListContentRequest(
        long libraryKey,
        PlexMediaType mediaType,
        int startIndex,
        int pageSize,
        bool includeStreams,
        bool includeMetadata = true,
        MediaType? requestMediaTypeOverride = null
    )
    {
        var requestMediaType = requestMediaTypeOverride ?? mediaType.ToMediaType();
        var addStreams = includeStreams
            && pageSize > 0
            && mediaType is PlexMediaType.Movie or PlexMediaType.Episode or PlexMediaType.MusicTrack or PlexMediaType.OtherVideos;
        return new ListContentRequest
        {
            MediaQuery = new MediaQuery
            {
                Type = requestMediaType,
                Sort = "titleSort:asc",
            },
            XPlexContainerStart = startIndex,
            XPlexContainerSize = pageSize,
            SectionId = libraryKey,
            Unwatched = null,
            IncludeGuids = includeMetadata ? BoolInt.True : null,
            IncludeMeta = includeMetadata ? BoolInt.True : null,
            IncludeOptionalElements = addStreams ? "Stream" : null,
            PmsApiVersion = addStreams ? "1.0" : null,
        };
    }

    private static async Task<Result<List<PlexLibraryMediaItem>>> ReadPhotoPagesAsync<T>(
        Func<int, Task<Result<T>>> readPage,
        Func<T, MediaContainerWithMetadataMediaContainer?> getContainer,
        PlexMediaType missingType,
        string description
    )
    {
        var items = new List<PlexLibraryMediaItem>();
        var offset = 0;
        var pages = new PhotoPageState();
        while (true)
        {
            var response = await readPage(offset);
            if (response.IsCancelledOrFailed)
                return response.ToResult<List<PlexLibraryMediaItem>>().LogIfFailed();

            var container = getContainer(response.Value);
            if (container is null)
                return Result.Fail<List<PlexLibraryMediaItem>>($"Plex returned no typed {description} container").LogError();

            var page = pages.Validate(container, offset, missingType, description);
            if (page.IsFailed)
                return page.ToResult<List<PlexLibraryMediaItem>>();

            items.AddRange(page.Value.Items);
            if (page.Value.Complete)
                return Result.Ok(items);
            offset = page.Value.NextOffset;
        }
    }

    private static IEnumerable<PlexLibraryMediaItem> Enumerate(
        MediaContainerWithMetadataMediaContainer container,
        PlexMediaType missingType
    )
    {
        foreach (var item in container.Directory ?? [])
            yield return NormalizePhotoItem(item, PlexLibraryMediaItemOrigin.Directory, missingType);
        foreach (var item in container.Metadata ?? [])
            yield return NormalizePhotoItem(item, PlexLibraryMediaItemOrigin.Metadata, missingType);
        foreach (var item in container.Photo ?? [])
            yield return NormalizePhotoItem(item, PlexLibraryMediaItemOrigin.Photo, missingType);
        foreach (var item in container.Video ?? [])
            yield return NormalizePhotoItem(item, PlexLibraryMediaItemOrigin.Video, missingType);
    }

    private static PlexLibraryMediaItem NormalizePhotoItem(
        Metadata metadata,
        PlexLibraryMediaItemOrigin origin,
        PlexMediaType missingType
    )
    {
        var type = metadata.Type?.Trim().ToLowerInvariant();
        var mediaType = type switch
        {
            null or "" when string.Equals(metadata.Subtype?.Trim(), "photo", StringComparison.OrdinalIgnoreCase)
                => PlexMediaType.PhotoImage,
            null or "" when origin == PlexLibraryMediaItemOrigin.Photo => PlexMediaType.PhotoImage,
            null or "" when origin == PlexLibraryMediaItemOrigin.Video => PlexMediaType.OtherVideos,
            null or "" when origin == PlexLibraryMediaItemOrigin.Directory => (PlexMediaType?)null,
            null or "" => missingType,
            "photo" or "image" => PlexMediaType.PhotoImage,
            "clip" or "video" or "movie" => PlexMediaType.OtherVideos,
            "photoalbum" or "album" => PlexMediaType.PhotoAlbum,
            _ => null,
        };
        PlexLibraryMediaItemShape shape;
        if (metadata.Media is { Count: > 0 })
            shape = PlexLibraryMediaItemShape.Asset;
        else if (
            IsPositiveRatingKey(metadata.RatingKey)
            && !string.IsNullOrWhiteSpace(metadata.Key)
            && type is null or "" or "photo" or "image" or "photoalbum" or "album" or "directory"
        )
            shape = PlexLibraryMediaItemShape.Album;
        else if (
            string.IsNullOrWhiteSpace(metadata.RatingKey)
            && !string.IsNullOrWhiteSpace(metadata.Key)
            && (origin == PlexLibraryMediaItemOrigin.Directory || type is null or "" or "folder" or "directory")
        )
            shape = PlexLibraryMediaItemShape.Folder;
        else
            shape = PlexLibraryMediaItemShape.Unknown;
        return new PlexLibraryMediaItem(metadata, origin, mediaType, shape);
    }

    private static bool IsPositiveRatingKey(string? value) =>
        int.TryParse(value, out var parsed) && parsed > 0;

    internal enum PlexLibraryMediaItemOrigin
    {
        Directory,
        Metadata,
        Photo,
        Video,
    }

    internal enum PlexLibraryMediaItemShape
    {
        Unknown,
        Folder,
        Album,
        Asset,
    }

    internal sealed record PlexLibraryMediaItem(
        Metadata Metadata,
        PlexLibraryMediaItemOrigin Origin,
        PlexMediaType? MediaType,
        PlexLibraryMediaItemShape Shape
    );

    private sealed class PhotoPageState
    {
        private long? _total;

        public Result<PhotoPageResult> Validate(
            MediaContainerWithMetadataMediaContainer container,
            int requestedOffset,
            PlexMediaType missingType,
            string description
        )
        {
            var items = Enumerate(container, missingType).ToList();
            if (container.Offset is < 0 || container.Size is < 0 || container.TotalSize is < 0)
                return Result.Fail<PhotoPageResult>($"Plex returned invalid {description} pagination values").LogError();

            var offset = container.Offset ?? requestedOffset;
            if (offset != requestedOffset)
                return Result.Fail<PhotoPageResult>(
                    $"Plex returned {description} offset {offset} for requested offset {requestedOffset}"
                ).LogError();
            if (container.Size is { } size && size != items.Count)
                return Result.Fail<PhotoPageResult>($"Plex returned a {description} size inconsistent with its item window").LogError();
            if (_total.HasValue && container.TotalSize is null)
                return Result.Fail<PhotoPageResult>($"Plex omitted the {description} total during pagination").LogError();
            if (_total.HasValue && container.TotalSize != _total)
                return Result.Fail<PhotoPageResult>($"Plex changed the reported {description} total during enumeration").LogError();
            _total ??= container.TotalSize;

            var total = _total;
            var end = (long)requestedOffset + items.Count;
            if (total is { } knownTotal && (requestedOffset > knownTotal || end > knownTotal))
                return Result.Fail<PhotoPageResult>(
                    $"Plex returned a {description} window beyond its reported total"
                ).LogError();
            if (items.Count == 0)
            {
                if (total is { } knownEmptyTotal && requestedOffset != knownEmptyTotal)
                    return Result.Fail<PhotoPageResult>($"Plex returned an incomplete {description} page").LogError();
                return Result.Ok(new PhotoPageResult(items, true, requestedOffset));
            }
            if (total is { } knownTotalAtEnd && end == knownTotalAtEnd)
                return Result.Ok(new PhotoPageResult(items, true, requestedOffset));
            return Result.Ok(new PhotoPageResult(items, false, checked((int)end)));
        }
    }

    private readonly record struct PhotoPageResult(
        IReadOnlyList<PlexLibraryMediaItem> Items,
        bool Complete,
        int NextOffset
    );
}

internal sealed record PlexLibraryMediaPage(IReadOnlyList<LibraryMediaItemDTO> Items, int? Total);

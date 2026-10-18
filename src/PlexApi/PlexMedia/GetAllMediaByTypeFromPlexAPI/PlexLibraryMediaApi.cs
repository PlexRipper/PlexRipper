using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;

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
                new ListContentRequest
                {
                    MediaQuery = new MediaQuery
                    {
                        Type = mediaType.ToMediaType(),
                        Sort = "titleSort:asc",
                    },
                    XPlexContainerStart = 0,
                    XPlexContainerSize = 0,
                    SectionId = libraryKey,
                    // The SDK's default False adds a watched-state filter; null includes all media.
                    Unwatched = null,
                }
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
        var includeStreams = pageSize > 0
            && mediaType is PlexMediaType.Movie or PlexMediaType.Episode or PlexMediaType.MusicTrack or PlexMediaType.OtherVideos;
        var response = await client
            .Content.ListContentAsync(
                new ListContentRequest
                {
                    MediaQuery = new MediaQuery
                    {
                        Type = mediaType.ToMediaType(),
                        Sort = "titleSort:asc",
                    },
                    XPlexContainerStart = startIndex,
                    XPlexContainerSize = pageSize,
                    SectionId = libraryKey,
                    Unwatched = null,
                    IncludeGuids = BoolInt.True,
                    IncludeMeta = BoolInt.True,
                    IncludeOptionalElements = includeStreams ? "Stream" : null,
                    PmsApiVersion = includeStreams ? "1.0" : null,
                }
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
}

internal sealed record PlexLibraryMediaPage(IReadOnlyList<LibraryMediaItemDTO> Items, int? Total);

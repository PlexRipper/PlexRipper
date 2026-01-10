using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Fetches detailed metadata (including Media, Parts, and Streams) for a batch of rating keys from a Plex server.
/// </summary>
/// <param name="PlexServerId">The ID of the Plex server to fetch metadata from.</param>
/// <param name="RatingKeys">Array of rating keys to fetch detailed metadata for.</param>
public record GetDetailMetadataByRatingKeysCommand(int PlexServerId, string[] RatingKeys)
    : ICommand<Result<List<LibraryMediaItemDTO>>>;

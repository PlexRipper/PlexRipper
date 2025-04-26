using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public interface IPlexApiMediaService
{
    Task<Result<List<LibraryMediaItemDTO>>> SyncMedia(
        PlexLibrary plexLibrary,
        PlexMediaType plexType,
        int batchSize = 1000,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    );
}

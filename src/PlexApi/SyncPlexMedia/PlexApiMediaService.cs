using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class PlexApiMediaService : IPlexApiMediaService
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly PlexApiWrapper _plexApiWrapper;

    public PlexApiMediaService(ILog log, IPlexRipperDbContext dbContext, PlexApiWrapper plexApiWrapper)
    {
        _log = log;
        _dbContext = dbContext;
        _plexApiWrapper = plexApiWrapper;
    }

    public async Task<Result<List<LibraryMediaItemDTO>>> SyncMedia(
        PlexLibrary plexLibrary,
        PlexMediaType plexType,
        int batchSize = 1000,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexLibrary.PlexServerId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnectionResult = await _dbContext.ChoosePlexServerConnection(
            plexLibrary.PlexServerId,
            cancellationToken
        );

        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        var plexServerConnection = plexServerConnectionResult.Value;

        var mediaList = new List<LibraryMediaItemDTO>();

        var index = 0;

        var startTime = DateTime.UtcNow; // Start time for estimation

        while (true)
        {
            // Retrieve the media for this library
            var result = await _plexApiWrapper.GetMetadataForLibraryAsync(
                plexServerConnection,
                tokenResult.Value,
                plexLibrary.Key,
                index,
                batchSize,
                plexType
            );

            if (result.IsFailed)
            {
                result.ToResult().LogError();
                break;
            }

            var mediaContainer = result.Value;
            var totalSize = mediaContainer.TotalSize;
            index += mediaContainer.Size;

            if (mediaContainer.Size == 0 || mediaContainer.TotalSize == 0)
            {
                _log.Warning(
                    "The library with name: {PlexLibraryName} contains no media to retrieve",
                    plexLibrary.Name
                );
                return Result.Ok(new List<LibraryMediaItemDTO>());
            }

            if (mediaContainer.Metadata is null)
            {
                ResultExtensions.IsNull(nameof(mediaContainer.Metadata)).LogError();
                break;
            }

            mediaList.AddRange(mediaContainer.Metadata.Select(x => x.ToMediaItemDTO()));

            // Estimate remaining time
            var elapsedTime = DateTime.UtcNow - startTime;
            var progress = (double)index / totalSize;
            var estimatedTotalTime = elapsedTime.TotalSeconds / progress;
            var remainingTime = TimeSpan.FromSeconds(estimatedTotalTime - elapsedTime.TotalSeconds);

            // Report progress
            action?.Invoke(
                new MediaSyncProgress
                {
                    Type = plexLibrary.Type,
                    Received = index,
                    Total = totalSize,
                    TimeRemaining = remainingTime,
                }
            );

            // If the size is less than the batch size, we have reached the end
            if (mediaContainer.Size < batchSize)
                break;

            if (index >= totalSize)
                break;
        }

        if (!mediaList.Any())
            return ResultExtensions.IsEmpty(nameof(mediaList));

        _log.Here()
            .Information(
                "Finished getting {MediaCount} media items from library with name {PlexLibraryName}  ",
                mediaList.Count,
                plexLibrary.Name
            );

        return Result.Ok(mediaList);
    }
}

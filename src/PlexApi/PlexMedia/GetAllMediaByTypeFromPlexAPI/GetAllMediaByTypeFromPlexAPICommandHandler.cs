using Data.Contracts;
using FastEndpoints;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;
using ILog = Logging.Interface.ILog;

namespace PlexRipper.PlexApi;

public record GetAllMediaByTypeFromPlexApiCommand(
    PlexLibrary PlexLibrary,
    PlexMediaType MediaType,
    int BatchSize = 1000,
    Action<MediaSyncProgress>? Action = null
) : ICommand<Result<List<LibraryMediaItemDTO>>>;

public class GetAllMediaByTypeFromPlexApiCommandHandler
    : ICommandHandler<GetAllMediaByTypeFromPlexApiCommand, Result<List<LibraryMediaItemDTO>>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    public GetAllMediaByTypeFromPlexApiCommandHandler(
        ILog log,
        IPlexRipperDbContext dbContext,
        IPlexApiClientFactory plexApiClientFactory
    )
    {
        _log = log;
        _dbContext = dbContext;
        _plexApiClientFactory = plexApiClientFactory;
    }

    public async Task<Result<List<LibraryMediaItemDTO>>> ExecuteAsync(
        GetAllMediaByTypeFromPlexApiCommand command,
        CancellationToken ct
    )
    {
        var plexLibrary = command.PlexLibrary;
        var mediaType = command.MediaType;
        var batchSize = command.BatchSize;
        var action = command.Action;

        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexLibrary.PlexServerId, ct);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnectionResult = await _dbContext.ChoosePlexServerConnection(plexLibrary.PlexServerId, ct);

        if (plexServerConnectionResult.IsFailed)
            return plexServerConnectionResult.ToResult();

        var plexServerConnection = plexServerConnectionResult.Value;

        var client = _plexApiClientFactory.CreateClient(
            tokenResult.Value,
            new PlexApiClientOptions
            {
                ConnectionUrl = plexServerConnection.Url,
                Timeout = 30,
                RetryCount = 3,
            }
        );

        var mediaList = new List<LibraryMediaItemDTO>();

        // Get the total size of the library
        var totalSizeResult = await GetLibraryMediaTotalSize(client, plexLibrary.Key, mediaType);

        if (totalSizeResult.IsFailed)
            return totalSizeResult.ToResult();

        var totalSize = totalSizeResult.Value;
        if (totalSize == 0)
        {
            _log.Warning("The library with name: {PlexLibraryName} contains no media to retrieve", plexLibrary.Name);
            return Result.Ok(mediaList);
        }

        // Retrieve the media for this library
        var index = 0;
        var startTime = DateTime.UtcNow; // Start time for estimation

        while (true)
        {
            var mediaListResult = await GetMetadataForLibraryAsync(
                client,
                plexLibrary.Key,
                index,
                batchSize,
                mediaType
            );
            if (mediaListResult.IsFailed)
            {
                mediaListResult.ToResult().LogError();
                break;
            }

            var rawMediaList = mediaListResult.Value;
            index += rawMediaList.Count;

            // We need to get the media details because the initial metadata we get from the library is not complete
            var ratingKeys = rawMediaList.Select(x => x.RatingKey).ToList();
            var detailResult = await GetDetailMetadataByRatingKeysAsync(client, ratingKeys, batchSize);
            if (detailResult.IsFailed)
            {
                detailResult.ToResult().LogError();
                SendProgress(mediaType, startTime, index, totalSize, action);
                continue;
            }

            mediaList.AddRange(detailResult.Value);

            SendProgress(mediaType, startTime, index, totalSize, action);

            // If the size is less than the batch size, we have reached the end
            if (rawMediaList.Count < batchSize)
                break;

            if (index >= totalSize)
                break;
        }

        _log.Here()
            .Information(
                "Finished getting {MediaCount} media items from library with name {PlexLibraryName}  ",
                mediaList.Count,
                plexLibrary.Name
            );

        return Result.Ok(mediaList);
    }

    private void SendProgress(
        PlexMediaType plexMediaType,
        DateTime startTime,
        int index,
        int totalSize,
        Action<MediaSyncProgress>? action = null
    )
    {
        // Estimate remaining time
        var elapsedTime = DateTime.UtcNow - startTime;
        var progress = (double)index / totalSize;
        var remainingTime = TimeSpan.Zero;
        if (progress > 0)
        {
            var estimatedTotalTime = elapsedTime.TotalSeconds / progress;
            remainingTime = TimeSpan.FromSeconds(estimatedTotalTime - elapsedTime.TotalSeconds);
        }

        // Report progress
        action?.Invoke(
            new MediaSyncProgress
            {
                Type = plexMediaType,
                Received = index,
                Total = totalSize,
                TimeRemaining = remainingTime,
            }
        );
    }

    /// <summary>
    /// Gets the total size of the media in the library.
    /// </summary>
    private async Task<Result<int>> GetLibraryMediaTotalSize(IPlexAPI client, string libraryKey, PlexMediaType type)
    {
        if (!int.TryParse(libraryKey, out var libraryKeyInt))
            return ResultExtensions.IsInvalidId(nameof(libraryKey), libraryKey).LogError();

        var response = await client
            .Library.GetLibraryItemsAsync(
                new GetLibraryItemsRequest
                {
                    SectionKey = libraryKeyInt,
                    XPlexContainerSize = 0,
                    XPlexContainerStart = 0,
                    Type = type.ToApiTypeEnum<GetLibraryItemsQueryParamType>(),
                }
            )
            .ToResponse();

        if (response.IsFailed)
            return response.ToResult();

        return Result.Ok(response.Value?.Object?.MediaContainer?.TotalSize ?? 0);
    }

    /// <summary>
    /// Gets all the root level media metadata contained in this Plex library. For movies, it's all movies, and for TV shows it's all the shows without seasons and episodes.
    /// <remarks>URL: {{SERVER_URL}}/library/sections/{{LIBRARY_KEY}}/all?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    public async Task<Result<List<LibraryMediaItemDTO>>> GetMetadataForLibraryAsync(
        IPlexAPI client,
        string libraryKey,
        int startIndex,
        int batchSize,
        PlexMediaType type
    )
    {
        if (!int.TryParse(libraryKey, out var libraryKeyInt))
            return ResultExtensions.IsInvalidId(nameof(libraryKey), libraryKey).LogError();

        var response = await client
            .Library.GetAllMediaLibraryAsync(
                new GetAllMediaLibraryRequest
                {
                    Type = type.ToApiTypeEnum<GetAllMediaLibraryQueryParamType>(),
                    SectionKey = libraryKeyInt,
                    IncludeMeta = GetAllMediaLibraryQueryParamIncludeMeta.Disable,
                    IncludeGuids = QueryParamIncludeGuids.Enable,
                    XPlexContainerStart = startIndex,
                    XPlexContainerSize = batchSize,
                }
            )
            .ToResponse();

        if (response.IsFailed)
            return response.ToResult();

        var mediaDataList = response.Value?.Object?.MediaContainer?.Metadata ?? [];
        if (!mediaDataList.Any())
            return ResultExtensions.IsNull(nameof(response.Value.Object.MediaContainer)).LogError();

        return Result.Ok(mediaDataList.Select(x => x.ToMediaItemDTO()).ToList());
    }

    public async Task<Result<List<LibraryMediaItemDTO>>> GetDetailMetadataByRatingKeysAsync(
        IPlexAPI client,
        List<string> ratingKeys,
        int batchSize = 50
    )
    {
        if (ratingKeys.Count == 0)
            return ResultExtensions.IsEmpty(nameof(ratingKeys)).LogError();

        var list = new List<LibraryMediaItemDTO>();
        foreach (var chunk in ratingKeys.Chunk(batchSize))
        {
            var response = await client
                .Library.GetMediaMetaDataAsync(new GetMediaMetaDataRequest { RatingKey = string.Join(",", chunk) })
                .ToResponse();

            if (response.IsFailed)
            {
                _log.Error(
                    "Failed to get metadata for rating keys: {RatingKeys}. Error: {Error}",
                    string.Join(",", chunk),
                    response.Errors
                );
                response.ToResult().LogError();
                continue;
            }

            var metaDataList = response.Value.Object?.MediaContainer?.Metadata ?? [];
            if (!metaDataList.Any())
            {
                ResultExtensions.IsNull(nameof(response.Value.Object.MediaContainer.Metadata)).LogError();
                continue;
            }

            list.AddRange(metaDataList.Select(x => x.ToMediaItemDTO()));
        }

        return Result.Ok(list);
    }
}

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

        var mediaList = new List<LibraryMediaItemDTO>();

        var index = 0;

        var startTime = DateTime.UtcNow; // Start time for estimation

        var client = _plexApiClientFactory.CreateClient(
            tokenResult.Value,
            new PlexApiClientOptions
            {
                ConnectionUrl = plexServerConnection.Url,
                Timeout = 30,
                RetryCount = 3,
            }
        );

        while (true)
        {
            // Retrieve the media for this library
            var result = await GetMetadataForLibraryAsync(client, plexLibrary.Key, index, batchSize, mediaType);
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

    /// <summary>
    /// Gets all the root level media metadata contained in this Plex library. For movies, it's all movies, and for TV-shows it's all the shows without seasons and episodes.
    /// <remarks>URL: {{SERVER_URL}}/library/sections/{{LIBRARY_KEY}}/all?X-Plex-Token={{SERVER_TOKEN}}</remarks>
    /// </summary>
    public async Task<Result<GetAllMediaLibraryMediaContainer>> GetMetadataForLibraryAsync(
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

        var value = response.Value?.Object?.MediaContainer ?? null;

        return value is null
            ? ResultExtensions.IsNull(nameof(response.Value.Object.MediaContainer)).LogError()
            : Result.Ok(value);
    }
}
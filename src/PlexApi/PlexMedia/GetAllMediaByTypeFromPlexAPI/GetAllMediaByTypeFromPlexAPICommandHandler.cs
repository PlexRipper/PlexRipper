using FastEndpoints;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public record GetAllMediaByTypeFromPlexApiCommand(
    PlexLibrary PlexLibrary,
    PlexMediaType MediaType,
    int BatchSize = 1000
) : ICommand<Result<List<LibraryMediaItemDTO>>>;

public class GetAllMediaByTypeFromPlexApiCommandHandler
    : ICommandHandler<GetAllMediaByTypeFromPlexApiCommand, Result<List<LibraryMediaItemDTO>>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ILibrarySyncProgressStore _librarySyncProgressStore;
    private readonly IPlexApiClientFactory _plexApiClientFactory;

    public GetAllMediaByTypeFromPlexApiCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ILibrarySyncProgressStore librarySyncProgressStore,
        IPlexApiClientFactory plexApiClientFactory
    )
    {
        _log = log.ForContext<GetAllMediaByTypeFromPlexApiCommandHandler>();
        _dbContext = dbContext;
        _librarySyncProgressStore = librarySyncProgressStore;
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
        var totalSizeResult = await GetLibraryMediaTotalCount(client, plexLibrary.Key, mediaType);

        if (totalSizeResult.IsFailed)
            return totalSizeResult.ToResult();

        var totalSize = totalSizeResult.Value;
        if (totalSize == 0)
        {
            _log.Here()
                .Warning("The library with name: {PlexLibraryName} contains no media to retrieve", plexLibrary.Name);
            return Result.Ok(mediaList);
        }

        // Retrieve the media for this library
        var startTime = DateTime.UtcNow; // Start time for estimation
        var progressIndex = 0;

        for (var index = 0; index < totalSize; index += batchSize)
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
            progressIndex += rawMediaList.Count;

            mediaList.AddRange(rawMediaList);
            await SendProgress(plexLibrary.Id, mediaType, startTime, progressIndex, totalSize);

            if (ct.IsCancellationRequested)
            {
                return ResultExtensions.TaskIsCancelled(nameof(GetAllMediaByTypeFromPlexApiCommand)).LogInformation();
            }
        }

        _log.Here()
            .Information(
                "Finished getting {MediaCount} media items from library with name {PlexLibraryName}  ",
                mediaList.Count,
                plexLibrary.Name
            );

        return Result.Ok(mediaList);
    }

    private async Task SendProgress(
        int plexLibraryId,
        PlexMediaType plexMediaType,
        DateTime startTime,
        int index,
        int totalSize
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
        await _librarySyncProgressStore.UpdateItemAsync(
            plexLibraryId,
            new LibraryProgressItem
            {
                MediaType = plexMediaType,
                Received = Math.Clamp(index, 0, totalSize),
                Total = totalSize,
                TimeRemaining = remainingTime,
            },
            CancellationToken.None
        );
    }

    /// <summary>
    /// Gets the total count of the media in the library.
    /// </summary>
    private async Task<Result<int>> GetLibraryMediaTotalCount(IPlexAPI client, string libraryKey, PlexMediaType type)
    {
        if (!int.TryParse(libraryKey, out var libraryKeyInt))
            return ResultExtensions.IsInvalidId(nameof(libraryKey), libraryKey).LogError();

        var response = await client
            .Content.ListContentAsync(
                new ListContentRequest
                {
                    XPlexContainerStart = 1,
                    XPlexContainerSize = 0,
                    SectionId = libraryKeyInt.ToString(),
                    MediaQuery = new MediaQuery { Type = type.ToPlexApiMediaType() },
                }
            )
            .ToResponse();

        if (response.IsFailed)
            return response.ToResult();

        var rawValue = response.Value?.MediaContainerWithMetadata?.MediaContainer?.TotalSize ?? 0;
        var safeValue = (int)Math.Max(0, Math.Min(rawValue, int.MaxValue));
        return Result.Ok(safeValue);
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
            .Content.ListContentAsync(
                new ListContentRequest
                {
                    XPlexContainerStart = startIndex,
                    XPlexContainerSize = batchSize,
                    SectionId = libraryKeyInt.ToString(),
                    IncludeGuids = BoolInt.True,
                    IncludeMeta = BoolInt.False,
                    MediaQuery = new MediaQuery { Type = type.ToPlexApiMediaType() },
                }
            )
            .ToResponse();

        if (response.IsFailed)
            return response.ToResult();

        var mediaDataList = response.Value?.MediaContainerWithMetadata?.MediaContainer?.Metadata ?? [];
        if (!mediaDataList.Any())
            return ResultExtensions.IsNull("MediaContainerWithMetadata.MediaContainer.Metadata").LogError();

        return Result.Ok(mediaDataList.Select(x => x.ToMediaItemDTO()).ToList());
    }
}

using Application.Contracts;
using Data.Contracts;
using Logging.Interface;
using PlexApi.Contracts;
using Settings.Contracts;

namespace PlexRipper.PlexApi;

/// <summary>
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class PlexApiService : IPlexApiService
{
    private readonly ILog _log;

    private readonly IPlexRipperDbContext _dbContext;
    private readonly IServerSettingsModule _serverSettingsModule;
    private readonly IPlexApiMediaService _plexApiMediaService;
    private readonly PlexApiWrapper _plexApiWrapper;

    public PlexApiService(
        ILog log,
        IPlexRipperDbContext dbContext,
        IServerSettingsModule serverSettingsModule,
        IPlexApiMediaService plexApiMediaService,
        PlexApiWrapper plexApiWrapper
    )
    {
        _log = log;
        _plexApiWrapper = plexApiWrapper;
        _dbContext = dbContext;
        _serverSettingsModule = serverSettingsModule;
        _plexApiMediaService = plexApiMediaService;
    }

    /// <inheritdoc />
    public async Task<Result<LibraryMetadata>> GetLibraryMediaAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    )
    {
        // Retrieve updated version of the PlexLibrary
        var plexLibraries = await GetLibrarySectionsAsync(
            plexLibrary.PlexServerId,
            cancellationToken: cancellationToken
        );

        if (plexLibraries.IsFailed)
            return plexLibraries.ToResult();

        var updatedPlexLibrary = plexLibraries.Value.Find(x => x.Key == plexLibrary.Key);
        if (updatedPlexLibrary is null)
            return ResultExtensions.IsNull(nameof(updatedPlexLibrary));

        updatedPlexLibrary.Id = plexLibrary.Id;
        updatedPlexLibrary.PlexServerId = plexLibrary.PlexServerId;

        // Set the default folder path id for the destination
        updatedPlexLibrary.DefaultDestinationId = updatedPlexLibrary.Type.ToDefaultDestinationFolderId();

        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            plexLibrary.Type,
            action: action,
            cancellationToken: cancellationToken
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        // Pre sort the media list
        var mediaList = mediaListResult.Value.OrderByNatural(x => x.TitleSort).ToList();

        // Determine how to map based on the Library type.
        switch (updatedPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                updatedPlexLibrary.Movies.AddRange(mediaList.ToPlexMovies());
                break;
            case PlexMediaType.TvShow:
                updatedPlexLibrary.TvShows.AddRange(mediaList.ToPlexTvShows());
                break;
            default:
                return Result.Fail($"Unknown PlexLibrary type: {updatedPlexLibrary.Type}").LogError();
        }

        return Result.Ok(
            new LibraryMetadata
            {
                Library = updatedPlexLibrary,
                Countries = mediaList.ToUniquePlexCountry(),
                Genres = mediaList.ToUniquePlexGenre(),
                Roles = mediaList.ToUniquePlexRole(),
            }
        );
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexTvShowSeason>>> GetAllSeasonsAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    )
    {
        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            PlexMediaType.Season,
            action: action,
            cancellationToken: cancellationToken
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowSeasons();
        return Result.Ok(mediaList);
    }

    public async Task<Result<List<PlexTvShowEpisode>>> GetAllEpisodesAsync(
        PlexLibrary plexLibrary,
        Action<MediaSyncProgress>? action = null,
        CancellationToken cancellationToken = default
    )
    {
        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            PlexMediaType.Episode,
            action: action,
            cancellationToken: cancellationToken
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowEpisodes();
        return Result.Ok(mediaList);
    }

    /// <inheritdoc />
    public async Task<Result<List<PlexLibrary>>> GetLibrarySectionsAsync(
        int plexServerId,
        int plexAccountId = 0,
        CancellationToken cancellationToken = default
    )
    {
        var tokenResult = await _dbContext.GetPlexServerTokenAsync(plexServerId, plexAccountId, cancellationToken);
        if (tokenResult.IsFailed)
            return tokenResult.ToResult();

        var plexServerConnection = await _dbContext.ChoosePlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        var plexServer = plexServerConnection.Value.PlexServer;

        if (plexServer is null)
            return ResultExtensions.EntityNotFound(nameof(PlexServer), plexServerId);

        return await _plexApiWrapper.GetAccessibleLibraryInPlexServerAsync(
            tokenResult.Value,
            plexServerConnection.Value
        );
    }
}

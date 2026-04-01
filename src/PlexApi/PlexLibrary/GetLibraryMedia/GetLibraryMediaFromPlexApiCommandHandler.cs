namespace Reaparr.PlexApi;

/// <summary>
/// Retrieves all media metadata from the PlexApi for a given <see cref="PlexLibrary"/> and returns it as a <see cref="LibraryMetadata"/>.
/// This service is an extra layer of abstraction to convert incoming DTO's from the PlexAPI to workable entities.
/// This was done in order to keep all PlexApi related DTO's in the infrastructure layer.
/// </summary>
public class GetLibraryMediaFromPlexApiCommandHandler
    : ICommandHandler<GetLibraryMediaFromPlexApiCommand, Result<LibraryMetadata>>
{
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILibrarySyncProgressStore _librarySyncProgressStore;

    public GetLibraryMediaFromPlexApiCommandHandler(
        ILogger log,
        ICommandExecutor commandExecutor,
        ILibrarySyncProgressStore librarySyncProgressStore
    )
    {
        _log = log.ForContext<GetLibraryMediaFromPlexApiCommandHandler>();
        _commandExecutor = commandExecutor;
        _librarySyncProgressStore = librarySyncProgressStore;
    }

    public async Task<Result<LibraryMetadata>> ExecuteAsync(
        GetLibraryMediaFromPlexApiCommand fromPlexApiCommand,
        CancellationToken ct
    )
    {
        var plexLibrary = fromPlexApiCommand.PlexLibrary;

        // Retrieve an updated version of the PlexLibrary
        var plexLibraries = await Result.Try(() =>
            _commandExecutor.Send(new GetLibrarySectionsCommand(plexLibrary.PlexServerId), ct)
        );

        if (plexLibraries.IsFailed)
            return plexLibraries.ToResult();

        var updatedPlexLibrary = plexLibraries.Value.Find(x => x.Key == plexLibrary.Key);
        if (updatedPlexLibrary is null)
        {
            _log.Here()
                .Error(
                    "Could not find Plex Library with key {PlexLibraryKey} (Id: {PlexLibraryId}) after refresh. The library has been deleted from the server",
                    plexLibrary.Key,
                    plexLibrary.Id
                );
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), plexLibrary.Id);
        }

        updatedPlexLibrary.Id = plexLibrary.Id;
        updatedPlexLibrary.PlexServerId = plexLibrary.PlexServerId;

        // Set the default folder path id for the destination
        updatedPlexLibrary.DefaultDestinationId = updatedPlexLibrary.Type.ToDefaultDestinationFolderId();

        // TODO: Handle other media types (Music, Photos, etc)
        if (updatedPlexLibrary.Type is not (PlexMediaType.Movie or PlexMediaType.TvShow))
            return Result.Ok(new LibraryMetadata(updatedPlexLibrary));

        await _librarySyncProgressStore.StartAsync(updatedPlexLibrary.Id, updatedPlexLibrary.Type, ct);

        var mediaListResult = await Result.Try(() =>
            _commandExecutor.Send(
                new GetAllMediaByTypeFromPlexApiCommand(updatedPlexLibrary, updatedPlexLibrary.Type),
                ct
            )
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        // Pre-sort the media list
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
            new LibraryMetadata(updatedPlexLibrary)
            {
                Countries = mediaList.SelectMany(x => x.Country).ToList(),
                Genres = mediaList.SelectMany(x => x.Genre).ToList(),
                Actors = mediaList.SelectMany(x => x.Role).ToList(),
            }
        );
    }
}

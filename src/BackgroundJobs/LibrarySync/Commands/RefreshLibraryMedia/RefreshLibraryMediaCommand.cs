namespace Reaparr.BackgroundJobs;

/// <summary>
/// Retrieves the new media metadata from the PlexApi and stores it in the database.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
/// <returns>Returns the PlexLibrary with the containing media.</returns>
public record RefreshLibraryMediaCommand(int PlexLibraryId) : ICommand<Result<PlexLibrary>>;

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand>
{
    public RefreshLibraryMediaCommandValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaCommandHandler : ICommandHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;

    public RefreshLibraryMediaCommandHandler(ILogger log, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = log.ForContext<RefreshLibraryMediaCommandHandler>();
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<PlexLibrary>> ExecuteAsync(RefreshLibraryMediaCommand command, CancellationToken ct)
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.Include(x => x.PlexServer)
            .FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, ct);

        if (plexLibrary is null)
            return ResultExtensions.EntityNotFound(nameof(plexLibrary), command.PlexLibraryId);

        // Phase 1: Retrieve top-level media belonging to this PlexLibrary
        var syncLibraryMediaResult = await _commandExecutor.Send(
            new GetLibraryMediaFromPlexApiCommand(plexLibrary),
            ct
        );

        if (syncLibraryMediaResult.IsFailed)
            return syncLibraryMediaResult.ToResult();

        // Phase 2: Insert the new / unique media metadata into the database
        var insertPlexLibraryMediaMetaDataResult = await _commandExecutor.Send(
            new InsertMediaMetaDataCommand(syncLibraryMediaResult.Value),
            ct
        );
        if (insertPlexLibraryMediaMetaDataResult.IsFailed)
            return insertPlexLibraryMediaMetaDataResult.LogError();

        // Phase 3: Add relations to the metadata such as Country, Actors and Genres for the library
        var syncPlexLibraryMediaMetaDataResult = await _commandExecutor.Send(
            new SyncPlexLibraryMediaMetaDataCommand(insertPlexLibraryMediaMetaDataResult.Value),
            ct
        );

        if (syncPlexLibraryMediaMetaDataResult.IsFailed)
            return syncPlexLibraryMediaMetaDataResult.LogError();

        // Phase 4: Continue with retrieving the rest of the media such as seasons/episodes based on the media type
        var newPlexLibrary = syncLibraryMediaResult.Value.Library;
        var refreshLibraryResult = newPlexLibrary.Type switch
        {
            PlexMediaType.Movie => await _commandExecutor.Send(
                new RefreshPlexMovieLibraryCommand(insertPlexLibraryMediaMetaDataResult.Value),
                ct
            ),
            PlexMediaType.TvShow => await _commandExecutor.Send(
                new RefreshPlexTvShowLibraryCommand(insertPlexLibraryMediaMetaDataResult.Value),
                ct
            ),
            _ => Result.Ok(newPlexLibrary),
        };

        if (newPlexLibrary.Type is not (PlexMediaType.Movie or PlexMediaType.TvShow))
        {
            _log.Here()
                .Warning(
                    "Library type {LibraryType} is currently not supported by Reaparr. Coming from library with id: {LibraryId}",
                    newPlexLibrary.Type,
                    plexLibrary.Id
                );

            return Result.Ok(newPlexLibrary);
        }

        if (refreshLibraryResult.IsFailed)
            return refreshLibraryResult;

        var syncedAt = DateTime.UtcNow;
        await _dbContext
            .PlexLibraries.Where(x => x.Id == command.PlexLibraryId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.SyncedAt, syncedAt).SetProperty(x => x.Outdated, false),
                ct
            );

        var syncedLibrary = await _dbContext.PlexLibraries.GetAsync(command.PlexLibraryId, ct);
        if (syncedLibrary is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), command.PlexLibraryId);

        return Result.Ok(syncedLibrary);
    }
}

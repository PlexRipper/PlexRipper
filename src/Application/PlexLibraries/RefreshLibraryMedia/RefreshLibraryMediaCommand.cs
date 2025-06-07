using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;
using WebAPI.Contracts;

namespace PlexRipper.Application;

/// <summary>
/// Retrieves the new media metadata from the PlexApi and stores it in the database.
/// </summary>
/// <param name="PlexLibraryId">The id of the <see cref="PlexLibrary"/> to retrieve.</param>
/// <param name="Action">The action to call for a progress update.</param>
/// <returns>Returns the PlexLibrary with the containing media.</returns>
public record RefreshLibraryMediaCommand(int PlexLibraryId, Action<LibraryProgress> Action)
    : ICommand<Result<PlexLibrary>>;

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand>
{
    public RefreshLibraryMediaCommandValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaCommandHandler : ICommandHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IRefreshLibraryProgressReporter _progressReporter;

    public RefreshLibraryMediaCommandHandler(
        IPlexRipperDbContext dbContext,
        ICommandExecutor commandExecutor,
        IRefreshLibraryProgressReporter progressReporter
    )
    {
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressReporter = progressReporter;
    }

    public async Task<Result<PlexLibrary>> ExecuteAsync(RefreshLibraryMediaCommand command, CancellationToken ct)
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.Include(x => x.PlexServer)
            .FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, ct);

        if (plexLibrary is null)
            return ResultExtensions.EntityNotFound(nameof(plexLibrary), command.PlexLibraryId);

        // Phase 1: Retrieve overview of all media belonging to this PlexLibrary
        var syncLibraryMediaResult = await _commandExecutor.Send(
            new GetLibraryMediaCommand(
                plexLibrary,
                progress =>
                    _progressReporter.SendProgress(
                        new RefreshLibraryProgressUpdate
                        {
                            PlexLibraryType = plexLibrary.Type,
                            PlexLibraryId = plexLibrary.Id,
                            Step = 1,
                            Percentage = progress.Percentage,
                            TimeRemaining = progress.TimeRemaining,
                            Action = command.Action,
                        }
                    )
            ),
            ct
        );

        if (syncLibraryMediaResult.IsFailed)
            return syncLibraryMediaResult.LogError();

        // Phase 2: Insert the media metadata into the database
        var insertPlexLibraryMediaMetaDataResult = await _commandExecutor.Send(
            new InsertMediaMetaDataCommand(syncLibraryMediaResult.Value),
            ct
        );
        if (insertPlexLibraryMediaMetaDataResult.IsFailed)
            return insertPlexLibraryMediaMetaDataResult.LogError();

        // Phase 3: Sync the metadata such as Country, Actors and Genres for the library
        var syncPlexLibraryMediaMetaDataResult = await _commandExecutor.Send(
            new SyncPlexLibraryMediaMetaDataCommand(insertPlexLibraryMediaMetaDataResult.Value),
            ct
        );

        if (syncPlexLibraryMediaMetaDataResult.IsFailed)
            return syncPlexLibraryMediaMetaDataResult.LogError();

        // Phase 4: Refresh the Plex library based on the media type
        var newPlexLibrary = syncLibraryMediaResult.Value.Library;
        switch (newPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                return await _commandExecutor.Send(
                    new RefreshPlexMovieLibraryCommand(insertPlexLibraryMediaMetaDataResult.Value, command.Action),
                    ct
                );
            case PlexMediaType.TvShow:
                return await _commandExecutor.Send(
                    new RefreshPlexTvShowLibraryCommand(insertPlexLibraryMediaMetaDataResult.Value, command.Action),
                    ct
                );
            default:
                return Result
                    .Fail($"Library type {newPlexLibrary.Type} is currently not supported by PlexRipper")
                    .LogWarning();
        }
    }
}

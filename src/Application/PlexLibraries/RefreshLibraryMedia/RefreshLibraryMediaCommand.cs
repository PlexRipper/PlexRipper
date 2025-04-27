using Data.Contracts;
using FluentValidation;
using Logging.Interface;
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
    : IRequest<Result<PlexLibrary>>;

public class RefreshLibraryMediaCommandValidator : AbstractValidator<RefreshLibraryMediaCommand>
{
    public RefreshLibraryMediaCommandValidator()
    {
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshLibraryMediaCommandHandler : IRequestHandler<RefreshLibraryMediaCommand, Result<PlexLibrary>>
{
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IRefreshLibraryProgressReporter _progressReporter;

    public RefreshLibraryMediaCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        ICommandExecutor commandExecutor,
        IRefreshLibraryProgressReporter progressReporter
    )
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _commandExecutor = commandExecutor;
        _progressReporter = progressReporter;
    }

    public async Task<Result<PlexLibrary>> Handle(
        RefreshLibraryMediaCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = await _dbContext
            .PlexLibraries.Include(x => x.PlexServer)
            .FirstOrDefaultAsync(x => x.Id == command.PlexLibraryId, cancellationToken);

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
            cancellationToken
        );

        if (syncLibraryMediaResult.IsFailed)
            return syncLibraryMediaResult.LogError();

        // Phase 2: Sync the metadata such as Country, Roles and Genres for the library
        var syncPlexLibraryMediaMetaDataResult = await _mediator.Send(
            new SyncPlexLibraryMediaMetaDataCommand(syncLibraryMediaResult.Value, plexLibrary.Id),
            cancellationToken
        );

        if (syncPlexLibraryMediaMetaDataResult.IsFailed)
            return syncPlexLibraryMediaMetaDataResult.LogError();

        var newPlexLibrary = syncLibraryMediaResult.Value.Library;

        switch (newPlexLibrary.Type)
        {
            case PlexMediaType.Movie:
                return await _commandExecutor.Send(
                    new RefreshPlexMovieLibraryCommand(newPlexLibrary, command.Action),
                    cancellationToken
                );
            case PlexMediaType.TvShow:
                return await _commandExecutor.Send(
                    new RefreshPlexTvShowLibraryCommand(newPlexLibrary, command.Action),
                    cancellationToken
                );
            default:
                return Result
                    .Fail($"Library type {newPlexLibrary.Type} is currently not supported by PlexRipper")
                    .LogWarning();
        }
    }
}

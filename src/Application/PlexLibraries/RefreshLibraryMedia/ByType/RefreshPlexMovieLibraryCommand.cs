using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public record RefreshPlexMovieLibraryCommand(
    InsertMediaMetaDataCommandResponse LibraryMetadata,
    Action<LibraryProgress> Action
) : ICommand<Result<PlexLibrary>>;

public class RefreshPlexMovieLibraryCommandValidator : AbstractValidator<RefreshPlexMovieLibraryCommand>
{
    public RefreshPlexMovieLibraryCommandValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibrary).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibraryId).GreaterThan(0);
    }
}

public class RefreshPlexMovieLibraryCommandHandler
    : ICommandHandler<RefreshPlexMovieLibraryCommand, Result<PlexLibrary>>
{
    private readonly ILog _log;
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IRefreshLibraryProgressReporter _progressReporter;

    public RefreshPlexMovieLibraryCommandHandler(
        ILog log,
        IMediator mediator,
        IPlexRipperDbContext dbContext,
        IRefreshLibraryProgressReporter progressReporter
    )
    {
        _log = log;
        _mediator = mediator;
        _dbContext = dbContext;
        _progressReporter = progressReporter;
    }

    public async Task<Result<PlexLibrary>> ExecuteAsync(
        RefreshPlexMovieLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = command.LibraryMetadata.PlexLibrary;

        if (plexLibrary.Movies.Any())
        {
            var i = 1;
            foreach (var plexMovie in plexLibrary.Movies)
                plexMovie.SortIndex = i++;

            var createResult = await _mediator.Send(
                new SyncPlexMoviesCommand(command.LibraryMetadata),
                cancellationToken
            );
            if (createResult.IsFailed)
            {
                await _progressReporter.SendProgress(
                    new RefreshLibraryProgressUpdate
                    {
                        Action = command.Action,
                        PlexLibraryType = PlexMediaType.Movie,
                        PlexLibraryId = plexLibrary.Id,
                        Step = 1,
                        Percentage = 1,
                    }
                );

                return createResult.ToResult().LogError();
            }
        }
        else
        {
            _log.Warning(
                "No Movies were found for library {PlexLibraryName} with id: {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );
        }

        // Phase 2 of 3: PlexLibrary media data was parsed successfully.
        await _progressReporter.SendProgress(
            new RefreshLibraryProgressUpdate
            {
                Action = command.Action,
                PlexLibraryType = PlexMediaType.Movie,
                PlexLibraryId = plexLibrary.Id,
                Step = 2,
                Percentage = 1,
            }
        );

        // Refresh the PlexLibrary from the database to ensure we have the latest data
        plexLibrary = await _dbContext.PlexLibraries.GetAsync(plexLibrary.Id, cancellationToken);
        var mediaSize = plexLibrary!.Movies.Sum(x => x.MediaSize);
        plexLibrary.SetMovieMetaData(plexLibrary.Movies.Count, mediaSize);

        if (plexLibrary.Movies.Any() && mediaSize == 0)
        {
            _log.Error(
                "No media size was found for library {PlexLibraryName} with id: {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );
        }

        // Mark the library as synced
        plexLibrary.SyncedAt = DateTime.UtcNow;

        await _dbContext.UpdatePlexLibraryById(plexLibrary, CancellationToken.None);

        _log.Information(
            "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
            plexLibrary.Title,
            plexLibrary.Id
        );

        // Phase 3 of 3: Movies have been successfully updated in the database.
        await _progressReporter.SendProgress(
            new RefreshLibraryProgressUpdate
            {
                Action = command.Action,
                PlexLibraryType = PlexMediaType.Movie,
                PlexLibraryId = plexLibrary.Id,
                Step = 3,
                Percentage = 1,
            }
        );

        return Result.Ok(plexLibrary);
    }
}

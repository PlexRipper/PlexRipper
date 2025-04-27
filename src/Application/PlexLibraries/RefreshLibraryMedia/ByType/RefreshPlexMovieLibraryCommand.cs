using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using WebAPI.Contracts;

namespace PlexRipper.Application;

public record RefreshPlexMovieLibraryCommand(PlexLibrary PlexLibrary, Action<LibraryProgress> Action)
    : ICommand<Result<PlexLibrary>>;

public class RefreshPlexMovieLibraryCommandValidator : AbstractValidator<RefreshPlexMovieLibraryCommand>
{
    public RefreshPlexMovieLibraryCommandValidator()
    {
        RuleFor(x => x.PlexLibrary).NotNull();
        RuleFor(x => x.PlexLibrary.Id).GreaterThan(0);
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
        var plexLibrary = command.PlexLibrary;

        if (plexLibrary.Movies.Any())
        {
            for (var i = 0; i < plexLibrary.Movies.Count; i++)
            {
                var plexMovie = plexLibrary.Movies[i];
                plexMovie.PlexLibraryId = plexLibrary.Id;
                plexMovie.PlexServerId = plexLibrary.PlexServerId;
                plexMovie.SortIndex = i + 1;
            }

            var createResult = await _mediator.Send(new SyncPlexMoviesCommand(plexLibrary.Movies));
            if (createResult.IsFailed)
            {
                _progressReporter.SendProgress(
                    new RefreshLibraryProgressUpdate()
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
        _progressReporter.SendProgress(
            new RefreshLibraryProgressUpdate()
            {
                Action = command.Action,
                PlexLibraryType = PlexMediaType.Movie,
                PlexLibraryId = plexLibrary.Id,
                Step = 2,
                Percentage = 1,
            }
        );

        var mediaSize = plexLibrary.Movies.Sum(x => x.MediaSize);
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
        _progressReporter.SendProgress(
            new RefreshLibraryProgressUpdate()
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

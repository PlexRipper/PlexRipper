using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;
using Reaparr.BackgroundJobs.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.BackgroundJobs;

public record RefreshPlexMovieLibraryCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : ICommand<Result<PlexLibrary>>;

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
    private readonly ILogger _log;
    private readonly ICommandExecutor _commandExecutor;
    private readonly IReaparrDbContext _dbContext;
    private readonly ILibrarySyncProgressStore _librarySyncProgressStore;

    public RefreshPlexMovieLibraryCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        ILibrarySyncProgressStore librarySyncProgressStore,
        ICommandExecutor commandExecutor
    )
    {
        _log = log.ForContext<RefreshPlexMovieLibraryCommandHandler>();
        _commandExecutor = commandExecutor;
        _dbContext = dbContext;
        _librarySyncProgressStore = librarySyncProgressStore;
    }

    public async Task<Result<PlexLibrary>> ExecuteAsync(
        RefreshPlexMovieLibraryCommand command,
        CancellationToken cancellationToken
    )
    {
        var plexLibrary = command.LibraryMetadata.PlexLibrary;
        var plexLibraryId = command.LibraryMetadata.PlexLibraryId;
        var movieCount = plexLibrary.Movies.Count;

        if (plexLibrary.Movies.Any())
        {
            var i = 1;
            foreach (var plexMovie in plexLibrary.Movies)
                plexMovie.SortIndex = i++;

            var syncResult = await _commandExecutor.Send(
                new SyncPlexMoviesCommand(command.LibraryMetadata),
                cancellationToken
            );

            if (syncResult.IsFailed)
            {
                // Report movies as not yet synced on failure
                await _librarySyncProgressStore.UpdateItemAsync(
                    plexLibrary.Id,
                    new LibraryProgressItem
                    {
                        MediaType = PlexMediaType.Movie,
                        Received = 0,
                        Total = movieCount,
                        TimeRemaining = TimeSpan.Zero,
                    }
                );

                return syncResult.ToResult().LogError();
            }
        }
        else
        {
            _log.Here()
                .Warning(
                    "No Movies were found for library {PlexLibraryName} with id: {PlexLibraryId}",
                    plexLibrary.Title,
                    plexLibrary.Id
                );
        }

        _log.Here()
            .Information(
                "Successfully refreshed library {PlexLibraryName} with id: {PlexLibraryId}",
                plexLibrary.Title,
                plexLibrary.Id
            );

        // Report movies as successfully synced
        await _librarySyncProgressStore.UpdateItemAsync(
            plexLibrary.Id,
            new LibraryProgressItem
            {
                MediaType = PlexMediaType.Movie,
                Received = movieCount,
                Total = movieCount,
                TimeRemaining = TimeSpan.Zero,
            }
        );

        // Refresh the PlexLibrary from the database to ensure we have the latest data
        var plexLibraryDb = await _dbContext.PlexLibraries.GetAsync(plexLibraryId, cancellationToken);
        return plexLibraryDb is null
            ? ResultExtensions.EntityNotFound(nameof(PlexLibrary), plexLibraryId)
            : Result.Ok(plexLibraryDb);
    }
}

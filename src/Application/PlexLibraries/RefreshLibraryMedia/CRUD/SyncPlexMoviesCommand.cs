using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SyncPlexMoviesCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : IRequest<Result<CrudMoviesReport>>;

public class SyncPlexMoviesCommandValidator : AbstractValidator<SyncPlexMoviesCommand>
{
    public SyncPlexMoviesCommandValidator(ILog<SyncPlexMoviesCommandValidator> log)
    {
        var stopWatch = Stopwatch.StartNew();
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibrary).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibrary.PlexServerId).GreaterThan(0);
        RuleFor(x => x.LibraryMetadata.PlexLibraryId).GreaterThan(0);

        RuleFor(x => x.LibraryMetadata.PlexLibrary.Movies).NotNull();
        RuleForEach(x => x.LibraryMetadata.PlexLibrary.Movies)
            .ChildRules(movie =>
            {
                movie.RuleFor(x => x.Key).GreaterThan(0);
            });

        stopWatch.StopAndLog($"Finished validating {nameof(SyncPlexMoviesCommand)}");
    }
}

public class SyncPlexMoviesCommandHandler : IRequestHandler<SyncPlexMoviesCommand, Result<CrudMoviesReport>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly CrudMoviesReport _report = new();

    private readonly BulkConfig? _config =
        new()
        {
            BatchSize = 500,
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
        };

    public SyncPlexMoviesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<CrudMoviesReport>> Handle(
        SyncPlexMoviesCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var plexLibraryId = command.LibraryMetadata.PlexLibraryId;
            var plexServerId = command.LibraryMetadata.PlexLibrary.PlexServerId;
            var libraryName = await _dbContext.GetPlexLibraryNameById(plexLibraryId, cancellationToken);

            _log.Debug(
                "Starting syncing of movies in library: {PlexLibraryName} with id: {PlexLibraryId} by first removing all media and then reinserting it",
                libraryName,
                plexLibraryId
            );

            var stopWatch = Stopwatch.StartNew();

            await RemoveMedia(plexLibraryId, cancellationToken);

            var plexMovies = command.LibraryMetadata.PlexLibrary.Movies.ToList();
            await _dbContext.BulkInsertPlexMoviesAsync(plexMovies, plexServerId, plexLibraryId, ct: cancellationToken);
            _report.CreatedMovies = plexMovies.Count;

            await SyncMovieActors(
                plexMovies,
                command.LibraryMetadata.PlexActors,
                plexLibraryId,
                libraryName,
                cancellationToken
            );
            await SyncMovieGenres(
                plexMovies,
                command.LibraryMetadata.PlexGenres,
                plexLibraryId,
                libraryName,
                cancellationToken
            );
            await SyncMovieCountries(
                plexMovies,
                command.LibraryMetadata.PlexCountries,
                plexLibraryId,
                libraryName,
                cancellationToken
            );

            stopWatch.Stop();

            _log.Information(
                "Finished media syncing plexLibrary: {PlexLibraryName} with id: {PlexLibraryId} in {TotalMilliseconds} milliseconds",
                libraryName,
                plexLibraryId,
                stopWatch.Elapsed.TotalMilliseconds
            );

            _log.DebugLine(_report.ToString());

            return Result.Ok(_report);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private async Task<Result> SyncMovieActors(
        List<PlexMovie> movies,
        Dictionary<int, PlexActor> plexActorsDict,
        int libraryId,
        string libraryName,
        CancellationToken ct
    )
    {
        _log.Debug(
            "Starting syncing of movie actors for library: {LibraryName} with id: {LibraryId}",
            libraryName,
            libraryId
        );
        var stopWatch = Stopwatch.StartNew();
        await _dbContext
            .PlexMovieActors.Where(x => x.PlexLibraryId == libraryId)
            .ExecuteDeleteAsync(cancellationToken: ct);

        var list = new List<PlexMovieActors>();
        var keyToIdDict = plexActorsDict.ToDictionary(kv => kv.Value.Key, kv => kv.Value.Id);

        foreach (var movie in movies)
        {
            foreach (var role in movie.Actors)
            {
                if (keyToIdDict.TryGetValue(role.Key, out var plexActorId))
                {
                    list.Add(new PlexMovieActors(plexActorId, libraryId, movie.Id));
                }
            }
        }

        var insertResult = await Result.Try(() => _dbContext.BulkInsertAsync(list, _config, ct));
        stopWatch.StopAndLog(
            $"Synced {list.Count} {nameof(PlexMovieActors)} for library: {libraryName} with id: {libraryId}"
        );

        return insertResult;
    }

    private async Task<Result> SyncMovieGenres(
        List<PlexMovie> movies,
        Dictionary<int, PlexGenre> plexGenreDict,
        int libraryId,
        string libraryName,
        CancellationToken ct
    )
    {
        _log.Debug(
            "Starting syncing of movie genres for library: {LibraryName} with id: {LibraryId}",
            libraryName,
            libraryId
        );
        var stopWatch = Stopwatch.StartNew();

        await _dbContext
            .PlexMovieGenres.Where(x => x.PlexLibraryId == libraryId)
            .ExecuteDeleteAsync(cancellationToken: ct);

        var list = new List<PlexMovieGenres>();
        var keyToIdDict = plexGenreDict.ToDictionary(kv => kv.Value.Key, kv => kv.Value.Id);

        foreach (var movie in movies)
        {
            foreach (var role in movie.Actors)
            {
                if (keyToIdDict.TryGetValue(role.Key, out var plexActorId))
                {
                    list.Add(new PlexMovieGenres(plexActorId, libraryId, movie.Id));
                }
            }
        }

        var insertResult = await Result.Try(() => _dbContext.BulkInsertAsync(list, _config, ct));

        stopWatch.StopAndLog(
            $"Synced {list.Count} {nameof(PlexMovieGenres)} for library: {libraryName} with id: {libraryId}"
        );

        return insertResult;
    }

    private async Task<Result> SyncMovieCountries(
        List<PlexMovie> movies,
        Dictionary<int, PlexCountry> plexCountryDict,
        int libraryId,
        string libraryName,
        CancellationToken ct
    )
    {
        _log.Debug(
            "Starting syncing of movie countries for library: {LibraryName} with id: {LibraryId}",
            libraryName,
            libraryId
        );
        var stopWatch = Stopwatch.StartNew();

        await _dbContext
            .PlexMovieCountries.Where(x => x.PlexLibraryId == libraryId)
            .ExecuteDeleteAsync(cancellationToken: ct);

        var list = new List<PlexMovieCountries>();
        var keyToIdDict = plexCountryDict.ToDictionary(kv => kv.Value.Key, kv => kv.Value.Id);

        foreach (var movie in movies)
        {
            foreach (var role in movie.Actors)
            {
                if (keyToIdDict.TryGetValue(role.Key, out var plexActorId))
                {
                    list.Add(new PlexMovieCountries(plexActorId, libraryId, movie.Id));
                }
            }
        }

        var insertResult = await Result.Try(() => _dbContext.BulkInsertAsync(list, _config, ct));

        stopWatch.StopAndLog(
            $"Synced {list.Count} {nameof(PlexMovieCountries)} for library: {libraryName} with id: {libraryId}"
        );

        return insertResult;
    }

    private async Task RemoveMedia(int plexLibraryId, CancellationToken cancellationToken)
    {
        await _dbContext
            .PlexMovieDataStreams.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext
            .PlexMovieDataParts.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext
            .PlexMovieData.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        // Then remove the movies
        _report.DeletedMovies = await _dbContext
            .PlexMovies.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public record CrudMoviesReport
{
    public int CreatedMovies { get; set; }

    public int UpdatedMovies { get; set; }

    public int DeletedMovies { get; set; }

    public override string ToString() =>
        $@"
        CreatedMovies: {CreatedMovies}
        UpdatedMovies: {UpdatedMovies}
        DeletedMovies: {DeletedMovies}";
}

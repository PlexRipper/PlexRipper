using System.Diagnostics;
using EFCore.BulkExtensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Serilog;

namespace Reaparr.Application;

public record SyncPlexMoviesCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : ICommand<Result<CrudMoviesReport>>;

public class SyncPlexMoviesCommandValidator : AbstractValidator<SyncPlexMoviesCommand>
{
    public SyncPlexMoviesCommandValidator()
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

        RuleFor(x => x.LibraryMetadata.PlexActors).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexGenres).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexCountries).NotNull();

        stopWatch.StopAndLog($"Finished validating {nameof(SyncPlexMoviesCommand)}");
    }
}

public class SyncPlexMoviesCommandHandler : ICommandHandler<SyncPlexMoviesCommand, Result<CrudMoviesReport>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    private readonly CrudMoviesReport _report = new();

    private readonly BulkConfig? _config = new() { BatchSize = 500, SetOutputIdentity = true };

    public SyncPlexMoviesCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<CrudMoviesReport>> ExecuteAsync(
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

            var plexLibrary = await _dbContext
                .PlexLibraries.AsTracking()
                .FirstOrDefaultAsync(x => x.Id == plexLibraryId, cancellationToken);

            var syncActorResult = await SyncMovieActors(
                plexMovies,
                command.LibraryMetadata.PlexActors,
                plexLibraryId,
                libraryName,
                cancellationToken
            );

            if (syncActorResult.IsSuccess)
                plexLibrary!.ActorsCount = syncActorResult.Value;

            var syncGenreResult = await SyncMovieGenres(
                plexMovies,
                command.LibraryMetadata.PlexGenres,
                plexLibraryId,
                libraryName,
                cancellationToken
            );

            if (syncGenreResult.IsSuccess)
                plexLibrary!.GenresCount = syncGenreResult.Value;

            var syncCountriesResult = await SyncMovieCountries(
                plexMovies,
                command.LibraryMetadata.PlexCountries,
                plexLibraryId,
                libraryName,
                cancellationToken
            );

            if (syncCountriesResult.IsSuccess)
                plexLibrary!.CountriesCount = syncCountriesResult.Value;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var mergeResult = Result.Merge(syncActorResult, syncGenreResult, syncCountriesResult);
            if (mergeResult.IsFailed)
            {
                _log.Error("Failed to sync movie metadata: {Error}", mergeResult.Errors);
                return mergeResult.LogError();
            }

            stopWatch.Stop();

            _log.Information(
                "Finished media syncing plexLibrary: {PlexLibraryName} with id: {PlexLibraryId} in {TotalMilliseconds} milliseconds",
                libraryName,
                plexLibraryId,
                stopWatch.Elapsed.TotalMilliseconds
            );

            _log.Debug(_report.ToString());

            return Result.Ok(_report);
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private async Task<Result<int>> SyncMovieActors(
        List<PlexMovie> movies,
        Dictionary<string, PlexActor> plexActorsDict,
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
            foreach (var actor in movie.Actors)
            {
                if (keyToIdDict.TryGetValue(actor.Key, out var plexActorId))
                {
                    list.Add(new PlexMovieActors(plexActorId, libraryId, movie.Id));
                }
            }
        }

        // Remove duplicates before inserting
        list = list.DistinctBy(x => new { x.PlexActorId, x.PlexMovieId }).ToList();

        var insertResult = await Result.Try(() => _dbContext.BulkInsertAsync(list, _config, ct));
        if (insertResult.IsFailed)
        {
            _log.Error("Failed to sync movie actors: {Error}", insertResult.Errors);
            return insertResult;
        }

        stopWatch.StopAndLog(
            $"Synced {list.Count} {nameof(PlexMovieActors)} for library: {libraryName} with id: {libraryId}"
        );

        return Result.Ok(list.Count);
    }

    private async Task<Result<int>> SyncMovieGenres(
        List<PlexMovie> movies,
        Dictionary<string, PlexGenre> plexGenreDict,
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
            foreach (var genre in movie.Genres)
            {
                if (keyToIdDict.TryGetValue(genre.Key, out var plexGenreId))
                {
                    list.Add(new PlexMovieGenres(plexGenreId, libraryId, movie.Id));
                }
            }
        }

        // Remove duplicates before inserting
        list = list.DistinctBy(x => new { x.GenresId, x.PlexMovieId }).ToList();

        var insertResult = await Result.Try(() => _dbContext.BulkInsertAsync(list, _config, ct));
        if (insertResult.IsFailed)
        {
            _log.Error("Failed to sync movie genres: {Error}", insertResult.Errors);
            return insertResult;
        }

        stopWatch.StopAndLog(
            $"Synced {list.Count} {nameof(PlexMovieGenres)} for library: {libraryName} with id: {libraryId}"
        );

        return Result.Ok(list.Count);
    }

    private async Task<Result<int>> SyncMovieCountries(
        List<PlexMovie> movies,
        Dictionary<string, PlexCountry> plexCountryDict,
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
            foreach (var country in movie.Countries)
            {
                if (keyToIdDict.TryGetValue(country.Key, out var plexCountryId))
                {
                    list.Add(new PlexMovieCountries(plexCountryId, libraryId, movie.Id));
                }
            }
        }

        // Remove duplicates before inserting
        list = list.DistinctBy(x => new { x.CountryId, x.PlexMovieId }).ToList();

        var insertResult = await Result.Try(() => _dbContext.BulkInsertAsync(list, _config, ct));
        if (insertResult.IsFailed)
        {
            _log.Error("Failed to sync movie countries: {Error}", insertResult.Errors);
            return insertResult;
        }

        stopWatch.StopAndLog(
            $"Synced {list.Count} {nameof(PlexMovieCountries)} for library: {libraryName} with id: {libraryId}"
        );

        return Result.Ok(list.Count);
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

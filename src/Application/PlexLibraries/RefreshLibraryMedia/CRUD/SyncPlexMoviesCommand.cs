using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SyncPlexMoviesCommand(List<PlexMovie> PlexMovies, int PlexServerId, int PlexLibraryId)
    : IRequest<Result<CrudMoviesReport>>;

public class SyncPlexMoviesCommandValidator : AbstractValidator<SyncPlexMoviesCommand>
{
    public SyncPlexMoviesCommandValidator(ILog<SyncPlexMoviesCommandValidator> log)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        RuleFor(x => x.PlexServerId).GreaterThan(0);
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.PlexMovies).NotNull();
        RuleForEach(x => x.PlexMovies)
            .ChildRules(tvShow =>
            {
                tvShow.RuleFor(x => x.Key).GreaterThan(0);
            });

        stopWatch.Stop();
        log.Here()
            .Debug(
                "Finished validating {ClassName} in {TotalMilliseconds} milliseconds",
                nameof(SyncPlexMoviesCommandValidator),
                stopWatch.Elapsed.TotalMilliseconds
            );
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
            var plexLibraryId = command.PlexLibraryId;
            var plexServerId = command.PlexServerId;
            var plexLibraryName = await _dbContext.GetPlexLibraryNameById(plexLibraryId, cancellationToken);

            _log.Debug(
                "Starting syncing of movies in library: {PlexLibraryName} with id: {PlexLibraryId} by first removing all media and then reinserting it",
                plexLibraryName,
                plexLibraryId
            );

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            await RemoveMedia(plexLibraryId, cancellationToken);

            var plexMovies = command.PlexMovies;
            await _dbContext.BulkInsertPlexMoviesAsync(plexMovies, plexServerId, plexLibraryId, ct: cancellationToken);
            _report.CreatedMovies = plexMovies.Count;

            await SyncMovieMetaData(plexMovies, plexLibraryId, plexLibraryName);

            stopWatch.Stop();

            _log.Information(
                "Finished media syncing plexLibrary: {PlexLibraryName} with id: {PlexLibraryId} in {TotalMilliseconds} milliseconds",
                plexLibraryName,
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

    private async Task SyncMovieMetaData(List<PlexMovie> plexMovies, int plexLibraryId, string libraryName)
    {
        _log.Debug("Starting syncing of movie metadata for library: {LibraryName}", libraryName);

        var roleDict = await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Include(x => x.Roles)
            .SelectMany(x => x.Roles)
            .ToDictionaryAsync(x => x.PlexKey, x => x.Id);

        // These are always small dictionaries, so no need to worry about performance
        var genreDict = await _dbContext.PlexGenres.ToDictionaryAsync(x => x.PlexKey, x => x.Id);
        var countryDict = await _dbContext.PlexCountries.ToDictionaryAsync(x => x.PlexKey, x => x.Id);

        var plexMovieRoles = new List<PlexMovieRoles>();
        var plexMovieGenres = new List<PlexMovieGenres>();
        var plexMovieCountries = new List<PlexMovieCountries>();

        foreach (var plexMovie in plexMovies)
        {
            foreach (var plexRole in plexMovie.Roles)
            {
                if (roleDict.TryGetValue(plexRole.PlexKey, out var roleId))
                    plexMovieRoles.Add(new PlexMovieRoles(roleId, plexLibraryId, plexMovie.Id));
            }

            foreach (var plexGenre in plexMovie.Genres)
            {
                if (genreDict.TryGetValue(plexGenre.PlexKey, out var genreId))
                    plexMovieGenres.Add(new PlexMovieGenres(genreId, plexLibraryId, plexMovie.Id));
            }

            foreach (var plexCountry in plexMovie.Countries)
            {
                if (countryDict.TryGetValue(plexCountry.PlexKey, out var countryId))
                    plexMovieCountries.Add(new PlexMovieCountries(countryId, plexLibraryId, plexMovie.Id));
            }
        }

        await _dbContext.BulkInsertAsync(
            plexMovieCountries.DistinctBy(x => new { x.PlexMovieId, x.CountryId }).ToList(),
            _config,
            CancellationToken.None
        );
        await _dbContext.BulkInsertAsync(
            plexMovieGenres.DistinctBy(x => new { x.PlexMovieId, x.GenresId }).ToList(),
            _config,
            CancellationToken.None
        );
        await _dbContext.BulkInsertAsync(
            plexMovieRoles.DistinctBy(x => new { x.PlexMovieId, x.RolesId }).ToList(),
            _config,
            CancellationToken.None
        );

        _log.Debug(
            "Finished syncing of Movie metadata for library {LibraryName} with id: {LibraryId}",
            libraryName,
            plexLibraryId
        );
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

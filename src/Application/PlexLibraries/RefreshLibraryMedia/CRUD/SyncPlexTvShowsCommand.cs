using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record SyncPlexTvShowsCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : IRequest<Result<CrudTvShowsReport>>;

public class SyncPlexTvShowsCommandValidator : AbstractValidator<SyncPlexTvShowsCommand>
{
    public SyncPlexTvShowsCommandValidator(ILog<SyncPlexTvShowsCommandValidator> log)
    {
        var stopWatch = Stopwatch.StartNew();
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibrary).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibrary.PlexServerId).GreaterThan(0);
        RuleFor(x => x.LibraryMetadata.PlexLibraryId).GreaterThan(0);

        RuleFor(x => x.LibraryMetadata.PlexLibrary.TvShows).NotNull();

        RuleForEach(x => x.LibraryMetadata.PlexLibrary.TvShows)
            .ChildRules(tvShow =>
            {
                tvShow.RuleFor(x => x.Key).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexServerId).GreaterThan(0);
                tvShow.RuleForEach(y => y.Seasons).NotNull();
                tvShow.RuleForEach(a => a.Seasons).NotEmpty();

                tvShow
                    .RuleForEach(y => y.Seasons)
                    .ChildRules(season =>
                    {
                        season.RuleFor(a => a.Key).GreaterThan(0);
                        season.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                        season.RuleFor(y => y.PlexServerId).GreaterThan(0);

                        season.RuleForEach(a => a.Episodes).NotNull();
                        season.RuleForEach(a => a.Episodes).NotEmpty();
                        season
                            .RuleForEach(a => a.Episodes)
                            .ChildRules(episode =>
                            {
                                episode.RuleFor(c => c.Key).GreaterThan(0);
                                season.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                                season.RuleFor(y => y.PlexServerId).GreaterThan(0);
                            });
                    });
            });

        stopWatch.Stop();
        log.Here()
            .Debug(
                "Finished validating {ClassName} in {TotalMilliseconds} milliseconds",
                nameof(SyncPlexTvShowsCommandValidator),
                stopWatch.Elapsed.TotalMilliseconds
            );
    }
}

public class SyncPlexTvShowsCommandHandler : IRequestHandler<SyncPlexTvShowsCommand, Result<CrudTvShowsReport>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly CrudTvShowsReport _report = new();

    private readonly BulkConfig? _config =
        new()
        {
            BatchSize = 1000,
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
            EnableStreaming = true,
            UseTempDB = true,
        };

    public SyncPlexTvShowsCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result<CrudTvShowsReport>> Handle(
        SyncPlexTvShowsCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var plexLibraryId = command.LibraryMetadata.PlexLibraryId;
            var plexLibraryName = _dbContext
                .PlexLibraries.Where(x => x.Id == plexLibraryId)
                .Select(x => x.Title)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(plexLibraryName))
                return ResultExtensions.EntityNotFound(nameof(command.LibraryMetadata.PlexLibrary), plexLibraryId);

            _log.Debug(
                "Starting syncing of tv shows in library: {PlexLibraryName} with id:  {PlexLibraryId} by first removing all media and then reinserting it",
                plexLibraryName,
                plexLibraryId
            );

            var stopWatch = Stopwatch.StartNew();

            await RemoveMedia(plexLibraryId, cancellationToken);

            var plexTvShows = command.LibraryMetadata.PlexLibrary.TvShows.ToList();

            await _dbContext.BulkInsertAsync(plexTvShows, _config, cancellationToken);
            _report.CreatedTvShows = plexTvShows.Count;

            // Sync metadata such as Countries, Roles and Genre
            var genreDict = command.LibraryMetadata.PlexGenres;
            var countryDict = command.LibraryMetadata.PlexCountries;
            var actorDict = command.LibraryMetadata.PlexActors;

            await Task.WhenAll(
                SyncTvShowGenres(plexTvShows, genreDict, plexLibraryId, plexLibraryName, cancellationToken),
                SyncTvShowCountries(plexTvShows, countryDict, plexLibraryId, plexLibraryName, cancellationToken),
                SyncTvShowActors(plexTvShows, actorDict, plexLibraryId, plexLibraryName, cancellationToken)
            );

            // Set the foreign keys (PlexTvShowId) in PlexSeason based on the inserted PlexTvShows
            var plexSeasons = plexTvShows
                .SelectMany(tvShow =>
                    tvShow.Seasons.Select(season =>
                    {
                        season.TvShowId = tvShow.Id;

                        foreach (var episode in season.Episodes)
                            episode.TvShowId = tvShow.Id;

                        return season;
                    })
                )
                .ToList();

            // Bulk insert PlexSeasons
            await _dbContext.BulkInsertAsync(plexSeasons, _config, cancellationToken);
            _report.CreatedSeasons = plexSeasons.Count;

            // Set the foreign keys (PlexSeasonId) in PlexEpisodes based on the inserted PlexSeasons
            var plexEpisodes = plexSeasons
                .SelectMany(season =>
                {
                    foreach (var episode in season.Episodes)
                        episode.TvShowSeasonId = season.Id;

                    return season.Episodes;
                })
                .ToList();

            // Bulk insert PlexEpisodes
            await _dbContext.BulkInsertAsync(plexEpisodes, _config, cancellationToken);
            _report.CreatedEpisodes = plexEpisodes.Count;

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

    private async Task<Result> SyncTvShowGenres(
        List<PlexTvShow> plexTvShows,
        Dictionary<int, PlexGenre> genreDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        _log.Debug(
            "Starting syncing of TV show genres for library: {LibraryName} with id: {LibraryId}",
            libraryName,
            plexLibraryId
        );
        var stopWatch = Stopwatch.StartNew();

        await _dbContext
            .PlexTvShowGenres.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        var plexTvShowGenres = new List<PlexTvShowGenres>();
        var keyToIdDict = genreDict.ToDictionary(kv => kv.Value.Key, kv => kv.Value.Id);

        foreach (var plexTvShow in plexTvShows)
        {
            foreach (var plexGenre in plexTvShow.Genres)
            {
                if (keyToIdDict.TryGetValue(plexGenre.Key, out var genreId))
                    plexTvShowGenres.Add(new PlexTvShowGenres(genreId, plexLibraryId, plexTvShow.Id));
            }
        }

        var distinctGenres = plexTvShowGenres.DistinctBy(x => new { x.PlexTvShowId, x.GenresId }).ToList();
        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertAsync(distinctGenres, _config, cancellationToken)
        );
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.GenresCount, distinctGenres.Count), cancellationToken);

        stopWatch.StopAndLog(
            $"Synced {plexTvShowGenres.Count} {nameof(PlexTvShowGenres)} for library: {libraryName} with id: {plexLibraryId}"
        );

        return insertResult;
    }

    private async Task<Result> SyncTvShowCountries(
        List<PlexTvShow> plexTvShows,
        Dictionary<int, PlexCountry> countryDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        _log.Debug(
            "Starting syncing of TV show countries for library: {LibraryName} with id: {LibraryId}",
            libraryName,
            plexLibraryId
        );
        var stopWatch = Stopwatch.StartNew();

        await _dbContext
            .PlexTvShowCountries.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        var plexTvShowCountries = new List<PlexTvShowCountries>();
        var keyToIdDict = countryDict.ToDictionary(kv => kv.Value.Key, kv => kv.Value.Id);

        foreach (var plexTvShow in plexTvShows)
        {
            foreach (var plexCountry in plexTvShow.Countries)
            {
                if (keyToIdDict.TryGetValue(plexCountry.Key, out var countryId))
                    plexTvShowCountries.Add(new PlexTvShowCountries(countryId, plexLibraryId, plexTvShow.Id));
            }
        }

        var distinctCountries = plexTvShowCountries.DistinctBy(x => new { x.PlexTvShowId, x.CountryId }).ToList();

        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertAsync(distinctCountries, _config, CancellationToken.None)
        );

        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.CountriesCount, distinctCountries.Count), cancellationToken);

        stopWatch.StopAndLog(
            $"Synced {plexTvShowCountries.Count} {nameof(PlexTvShowCountries)} for library: {libraryName} with id: {plexLibraryId}"
        );

        return insertResult;
    }

    private async Task<Result> SyncTvShowActors(
        List<PlexTvShow> plexTvShows,
        Dictionary<int, PlexActor> actorDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        _log.Debug(
            "Starting syncing of TV show actors for library: {LibraryName} with id: {LibraryId}",
            libraryName,
            plexLibraryId
        );
        var stopWatch = Stopwatch.StartNew();

        await _dbContext
            .PlexTvShowActors.Where(x => x.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        var plexTvShowRoles = new List<PlexTvShowActors>();
        var keyToIdDict = actorDict.ToDictionary(kv => kv.Value.Key, kv => kv.Value.Id);

        foreach (var plexTvShow in plexTvShows)
        {
            foreach (var actor in plexTvShow.Actors)
            {
                if (keyToIdDict.TryGetValue(actor.Key, out var plexActorId))
                    plexTvShowRoles.Add(new PlexTvShowActors(plexActorId, plexLibraryId, plexTvShow.Id));
            }
        }

        var distinctActors = plexTvShowRoles.DistinctBy(x => new { x.PlexTvShowId, RolesId = x.PlexActorId }).ToList();
        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertAsync(distinctActors, _config, cancellationToken)
        );
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.ActorsCount, distinctActors.Count), cancellationToken);

        stopWatch.StopAndLog(
            $"Synced {plexTvShowRoles.Count} {nameof(PlexTvShowActors)} for library: {libraryName} with id: {plexLibraryId}"
        );

        return insertResult;
    }

    private async Task RemoveMedia(int plexLibraryId, CancellationToken cancellationToken)
    {
        _report.DeletedEpisodes = await _dbContext
            .PlexTvShowEpisodes.Where(e => e.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        _report.DeletedSeasons = await _dbContext
            .PlexTvShowSeason.Where(s => s.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        _report.DeletedTvShows = await _dbContext
            .PlexTvShows.Where(tv => tv.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}

public record CrudTvShowsReport
{
    public int CreatedTvShows { get; set; }

    public int UpdatedTvShows { get; set; }

    public int DeletedTvShows { get; set; }

    public int CreatedSeasons { get; set; }

    public int UpdatedSeasons { get; set; }

    public int DeletedSeasons { get; set; }

    public int CreatedEpisodes { get; set; }

    public int UpdatedEpisodes { get; set; }

    public int DeletedEpisodes { get; set; }

    public override string ToString() =>
        $@"
        CreatedTvShows: {CreatedTvShows}
        UpdatedTvShows: {UpdatedTvShows}
        DeletedTvShows: {DeletedTvShows}
        CreatedSeasons: {CreatedSeasons}
        UpdatedSeasons: {UpdatedSeasons}
        DeletedSeasons: {DeletedSeasons}
        CreatedEpisodes: {CreatedEpisodes}
        UpdatedEpisodes: {UpdatedEpisodes}
        DeletedEpisodes: {DeletedEpisodes}";
}

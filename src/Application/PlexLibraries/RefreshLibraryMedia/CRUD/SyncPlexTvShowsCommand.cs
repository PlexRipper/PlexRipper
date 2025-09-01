using System.Diagnostics;
using EFCore.BulkExtensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Application;

public record SyncPlexTvShowsCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : ICommand<Result<BulkInsertTvShowsRapport>>;

public class SyncPlexTvShowsCommandValidator : AbstractValidator<SyncPlexTvShowsCommand>
{
    public SyncPlexTvShowsCommandValidator(ILogger log)
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

public class SyncPlexTvShowsCommandHandler : ICommandHandler<SyncPlexTvShowsCommand, Result<BulkInsertTvShowsRapport>>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    private readonly BulkConfig? _config = new()
    {
        BatchSize = 1000,
        SetOutputIdentity = true,
        PreserveInsertOrder = true,
        CalculateStats = true,
        EnableStreaming = true,
        UseTempDB = true,
    };

    public SyncPlexTvShowsCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<SyncPlexTvShowsCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<BulkInsertTvShowsRapport>> ExecuteAsync(
        SyncPlexTvShowsCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var plexLibraryId = command.LibraryMetadata.PlexLibraryId;
            var plexLibraryName = await _dbContext.GetPlexLibraryNameById(
                plexLibraryId,
                cancellationToken: cancellationToken
            );
            var plexServerId = await _dbContext.GetPlexServerIdFromPlexLibraryId(plexLibraryId);

            if (string.IsNullOrWhiteSpace(plexLibraryName))
                return ResultExtensions.EntityNotFound(nameof(command.LibraryMetadata.PlexLibrary), plexLibraryId);

            _log.Here()
                .Debug(
                    "Starting syncing of tv shows in library: {PlexLibraryName} with id:  {PlexLibraryId} by first removing all media and then reinserting it",
                    plexLibraryName,
                    plexLibraryId
                );

            var stopWatch = Stopwatch.StartNew();

            var removeRapport = await RemoveMedia(plexLibraryId, cancellationToken);

            var plexTvShows = command.LibraryMetadata.PlexLibrary.TvShows.ToList();

            var bulkInsertRapportResult = await _dbContext.BulkInsertPlexTvShowsAsync(
                plexTvShows,
                plexServerId,
                plexLibraryId,
                cancellationToken
            );

            if (bulkInsertRapportResult.IsFailed)
                return bulkInsertRapportResult;

            // Map the removed media counts to the bulk insert rapport
            bulkInsertRapportResult.Value.DeletedTvShows = removeRapport.DeletedTvShows;
            bulkInsertRapportResult.Value.DeletedSeasons = removeRapport.DeletedSeasons;
            bulkInsertRapportResult.Value.DeletedEpisodes = removeRapport.DeletedEpisodes;

            // Sync metadata such as Countries, Roles and Genre
            var genreDict = command.LibraryMetadata.PlexGenres;
            var countryDict = command.LibraryMetadata.PlexCountries;
            var actorDict = command.LibraryMetadata.PlexActors;

            await Task.WhenAll(
                SyncTvShowGenres(plexTvShows, genreDict, plexLibraryId, plexLibraryName, cancellationToken),
                SyncTvShowCountries(plexTvShows, countryDict, plexLibraryId, plexLibraryName, cancellationToken),
                SyncTvShowActors(plexTvShows, actorDict, plexLibraryId, plexLibraryName, cancellationToken)
            );

            stopWatch.Stop();

            _log.Here()
                .Information(
                    "Finished media syncing plexLibrary: {PlexLibraryName} with id: {PlexLibraryId} in {TotalMilliseconds} milliseconds",
                    plexLibraryName,
                    plexLibraryId,
                    stopWatch.Elapsed.TotalMilliseconds
                );

            _log.Here().Debug(bulkInsertRapportResult.Value.ToString());

            return bulkInsertRapportResult;
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private async Task<Result> SyncTvShowGenres(
        List<PlexTvShow> plexTvShows,
        Dictionary<string, PlexGenre> genreDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        _log.Here()
            .Debug(
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
        var insertResult = await Result.Try(() =>
            _dbContext.BulkInsertAsync(distinctGenres, _config, cancellationToken)
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
        Dictionary<string, PlexCountry> countryDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        _log.Here()
            .Debug(
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

        var insertResult = await Result.Try(() =>
            _dbContext.BulkInsertAsync(distinctCountries, _config, CancellationToken.None)
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
        Dictionary<string, PlexActor> actorDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        _log.Here()
            .Debug(
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
        var insertResult = await Result.Try(() =>
            _dbContext.BulkInsertAsync(distinctActors, _config, cancellationToken)
        );
        await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.ActorsCount, distinctActors.Count), cancellationToken);

        stopWatch.StopAndLog(
            $"Synced {plexTvShowRoles.Count} {nameof(PlexTvShowActors)} for library: {libraryName} with id: {plexLibraryId}"
        );

        return insertResult;
    }

    private async Task<BulkInsertTvShowsRapport> RemoveMedia(int plexLibraryId, CancellationToken cancellationToken)
    {
        await _dbContext
            .PlexTvShowEpisodeDataStreams.Where(e => e.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext
            .PlexTvShowEpisodeDataParts.Where(e => e.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        await _dbContext
            .PlexTvShowEpisodeData.Where(e => e.PlexLibraryId == plexLibraryId)
            .ExecuteDeleteAsync(cancellationToken);

        return new BulkInsertTvShowsRapport
        {
            DeletedEpisodes = await _dbContext
                .PlexTvShowEpisodes.Where(e => e.PlexLibraryId == plexLibraryId)
                .ExecuteDeleteAsync(cancellationToken),
            DeletedSeasons = await _dbContext
                .PlexTvShowSeason.Where(s => s.PlexLibraryId == plexLibraryId)
                .ExecuteDeleteAsync(cancellationToken),
            DeletedTvShows = await _dbContext
                .PlexTvShows.Where(tv => tv.PlexLibraryId == plexLibraryId)
                .ExecuteDeleteAsync(cancellationToken),
        };
    }
}

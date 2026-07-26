using EFCore.BulkExtensions;

namespace Reaparr.BackgroundJobs;

/// <summary>
/// Syncs the PlexTvShows of a PlexLibrary by first deleting all existing media and then reinserting the new media.
/// In addition to syncing the PlexTvShows, also syncs the related entities such as actors, genres and countries.
/// </summary>
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
                tvShow.RuleFor(x => x.PlexApiRatingKey).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                tvShow.RuleFor(y => y.PlexServerId).GreaterThan(0);
                tvShow.RuleForEach(y => y.Seasons).NotNull();
                tvShow.RuleForEach(a => a.Seasons).NotEmpty();

                tvShow
                    .RuleForEach(y => y.Seasons)
                    .ChildRules(season =>
                    {
                        season.RuleFor(a => a.PlexApiRatingKey).GreaterThan(0);
                        season.RuleFor(y => y.PlexLibraryId).GreaterThan(0);
                        season.RuleFor(y => y.PlexServerId).GreaterThan(0);

                        season.RuleForEach(a => a.Episodes).NotNull();
                        season.RuleForEach(a => a.Episodes).NotEmpty();
                        season
                            .RuleForEach(a => a.Episodes)
                            .ChildRules(episode =>
                            {
                                episode.RuleFor(c => c.PlexApiRatingKey).GreaterThan(0);
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
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;
    private readonly IReaparrDbContextFactory _dbContextFactory;

    private static BulkConfig CreateBulkConfig() =>
        new()
        {
            BatchSize = 1000,
            SetOutputIdentity = true,
            PreserveInsertOrder = true,
            CalculateStats = true,
            EnableStreaming = true,
            UseTempDB = true,
        };

    public SyncPlexTvShowsCommandHandler(
        ILogger log,
        IReaparrDbContext dbContext,
        IReaparrDbContextFactory dbContextFactory
    )
    {
        _log = log.ForContext<SyncPlexTvShowsCommandHandler>();
        _dbContext = dbContext;
        _dbContextFactory = dbContextFactory;
    }

    public async Task<Result<BulkInsertTvShowsRapport>> ExecuteAsync(
        SyncPlexTvShowsCommand command,
        CancellationToken cancellationToken
    )
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

        var removeRapport = await RemoveMedia(plexLibraryId, CancellationToken.None);

        var plexTvShows = command.LibraryMetadata.PlexLibrary.TvShows.ToList();

        var bulkInsertRapportResult = await Result.Try(() =>
            _dbContext.BulkInsertPlexTvShowsAsync(plexTvShows, plexServerId, plexLibraryId, cancellationToken)
        );

        if (bulkInsertRapportResult.IsFailed)
        {
            stopWatch.Stop();
            return bulkInsertRapportResult.LogError();
        }

        var bulkInsertRapport = bulkInsertRapportResult.Value;
        bulkInsertRapport.DeletedTvShows = removeRapport.DeletedTvShows;
        bulkInsertRapport.DeletedSeasons = removeRapport.DeletedSeasons;
        bulkInsertRapport.DeletedEpisodes = removeRapport.DeletedEpisodes;

        // Update counts in PlexLibrary from persisted rows so denormalized metrics cannot drift.
        var mediaSize = plexTvShows.Sum(x => x.MediaSize);
        var metrics = await _dbContext
            .PlexLibraries.Where(x => x.Id == plexLibraryId)
            .Select(_ => new
            {
                TvShowCount = _dbContext.PlexTvShows.Count(x => x.PlexLibraryId == plexLibraryId),
                SeasonCount = _dbContext.PlexTvShowSeason.Count(x => x.PlexLibraryId == plexLibraryId),
                EpisodeCount = _dbContext.PlexTvShowEpisodes.Count(x => x.PlexLibraryId == plexLibraryId),
            })
            .FirstAsync(cancellationToken);

        await _dbContext.SetTvShowMediaMetrics(
            plexLibraryId,
            metrics.TvShowCount,
            metrics.SeasonCount,
            metrics.EpisodeCount,
            mediaSize
        );

        // Sync metadata such as Countries, Roles and Genre using separate DbContext instances
        // to avoid EF Core DbContext thread-safety issues when running in parallel.
        var genreDict = command.LibraryMetadata.PlexGenres;
        var countryDict = command.LibraryMetadata.PlexCountries;
        var actorDict = command.LibraryMetadata.PlexActors;

        ResultBase[] results = await Task.WhenAll(
            SyncTvShowGenres(plexTvShows, genreDict, plexLibraryId, plexLibraryName, cancellationToken),
            SyncTvShowCountries(plexTvShows, countryDict, plexLibraryId, plexLibraryName, cancellationToken),
            SyncTvShowActors(plexTvShows, actorDict, plexLibraryId, plexLibraryName, cancellationToken)
        );

        var mergeResult = Result.Merge(results);
        if (mergeResult.IsFailed)
            return mergeResult.LogError();

        stopWatch.StopAndLog($"Finished media syncing plexLibrary: {plexLibraryName} with id: {plexLibraryId}");

        _log.Here().Debug(bulkInsertRapport.ToString());

        return Result.Ok(bulkInsertRapport);
    }

    private async Task<Result> SyncTvShowGenres(
        List<PlexTvShow> plexTvShows,
        Dictionary<string, PlexGenre> genreDict,
        int plexLibraryId,
        string libraryName,
        CancellationToken cancellationToken
    )
    {
        using var dbContext = await _dbContextFactory.CreateAsync();

        _log.Here()
            .Debug(
                "Starting syncing of TV show genres for library: {LibraryName} with id: {LibraryId}",
                libraryName,
                plexLibraryId
            );
        var stopWatch = Stopwatch.StartNew();

        await dbContext
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
            dbContext.BulkInsertAsync(distinctGenres, CreateBulkConfig(), cancellationToken)
        );
        await dbContext
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
        using var dbContext = await _dbContextFactory.CreateAsync();

        _log.Here()
            .Debug(
                "Starting syncing of TV show countries for library: {LibraryName} with id: {LibraryId}",
                libraryName,
                plexLibraryId
            );
        var stopWatch = Stopwatch.StartNew();

        await dbContext
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
            dbContext.BulkInsertAsync(distinctCountries, CreateBulkConfig(), cancellationToken)
        );

        await dbContext
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
        using var dbContext = await _dbContextFactory.CreateAsync();

        _log.Here()
            .Debug(
                "Starting syncing of TV show actors for library: {LibraryName} with id: {LibraryId}",
                libraryName,
                plexLibraryId
            );
        var stopWatch = Stopwatch.StartNew();

        await dbContext
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
            dbContext.BulkInsertAsync(distinctActors, CreateBulkConfig(), cancellationToken)
        );
        await dbContext
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

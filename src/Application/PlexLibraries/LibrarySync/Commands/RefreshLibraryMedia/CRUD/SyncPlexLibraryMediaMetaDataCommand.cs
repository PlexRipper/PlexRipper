using EFCore.BulkExtensions;

namespace Reaparr.Application;

/// <summary>
/// Command to synchronize Plex library media metadata (actors, genres, countries) for a
/// specific Plex library. The synchronization is a destructive replace: all existing
/// relations for the target library are removed and then recreated from the provided
/// <paramref name="LibraryMetadata"/> collections.
/// </summary>
/// <param name="LibraryMetadata">Aggregated metadata for the target Plex library.</param>
/// <returns>Returns the result of the operation.</returns>
public record SyncPlexLibraryMediaMetaDataCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : ICommand<Result>;

public class SyncPlexLibraryMediaMetaDataCommandValidator : Validator<SyncPlexLibraryMediaMetaDataCommand>
{
    public SyncPlexLibraryMediaMetaDataCommandValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.LibraryMetadata.PlexActors).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexGenres).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexCountries).NotNull();
    }
}

public class SyncPlexLibraryMediaMetaDataCommandHandler : ICommandHandler<SyncPlexLibraryMediaMetaDataCommand, Result>
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    private readonly BulkConfig? _bulkInsertConfig = new()
    {
        SetOutputIdentity = false,
        PreserveInsertOrder = true,
        UseTempDB = true,
    };

    public SyncPlexLibraryMediaMetaDataCommandHandler(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log.ForContext<SyncPlexLibraryMediaMetaDataCommandHandler>();
    }

    public async Task<Result> ExecuteAsync(SyncPlexLibraryMediaMetaDataCommand command, CancellationToken ct)
    {
        var libraryId = command.LibraryMetadata.PlexLibraryId;

        _log.Here().Debug("[SyncMetaData] ExecuteAsync entered for libraryId {LibraryId}", libraryId);

        // First, verify the library exists
        _log.Here().Debug("[SyncMetaData] Fetching library {LibraryId} from DB", libraryId);
        var library = await _dbContext.PlexLibraries.GetAsync(libraryId, cancellationToken: ct);
        if (library == null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        _log.Here().Debug("[SyncMetaData] Library found, fetching name");
        var roles = command.LibraryMetadata.PlexActors;
        var genres = command.LibraryMetadata.PlexGenres;
        var countries = command.LibraryMetadata.PlexCountries;

        var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId, ct);
        _log.Here()
            .Debug(
                "[SyncMetaData] Got library name '{LibraryName}'. Actors={ActorCount}, Genres={GenreCount}, Countries={CountryCount}",
                libraryName,
                roles.Count,
                genres.Count,
                countries.Count
            );

        _log.Here().Debug("[SyncMetaData] Starting SyncGenres");
        var syncGenresResult = await SyncGenres(genres, libraryId, libraryName, ct);
        _log.Here().Debug("[SyncMetaData] SyncGenres done. IsFailed={IsFailed}", syncGenresResult.IsFailed);

        _log.Here().Debug("[SyncMetaData] Starting SyncCountries");
        var syncCountriesResult = await SyncCountries(countries, libraryId, libraryName, ct);
        _log.Here().Debug("[SyncMetaData] SyncCountries done. IsFailed={IsFailed}", syncCountriesResult.IsFailed);

        _log.Here().Debug("[SyncMetaData] Starting SyncRoles");
        var syncRolesResult = await SyncRoles(roles, libraryId, libraryName, ct);
        _log.Here().Debug("[SyncMetaData] SyncRoles done. IsFailed={IsFailed}", syncRolesResult.IsFailed);

        var actorsCount = syncRolesResult.ValueOrDefault;
        var genresCount = syncGenresResult.ValueOrDefault;
        var countriesCount = syncCountriesResult.ValueOrDefault;

        _log.Here()
            .Debug(
                "[SyncMetaData] Calling SetLibraryMetaData: actors={ActorCount}, genres={GenreCount}, countries={CountryCount}",
                actorsCount,
                genresCount,
                countriesCount
            );
        await _dbContext.SetLibraryMetaData(libraryId, actorsCount, genresCount, countriesCount);
        _log.Here().Debug("[SyncMetaData] SetLibraryMetaData done");

        return Result.Merge(syncGenresResult, syncCountriesResult, syncRolesResult).ToResult();
    }

    private async Task<Result<int>> SyncRoles(
        Dictionary<string, PlexActor> sourceDict,
        int libraryId,
        string libraryName,
        CancellationToken ct
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started syncing {Count} roles for library {LibraryName}", sourceDict.Count, libraryName);

        if (!sourceDict.Any())
        {
            _log.Here()
                .Warning(
                    "No {NameOfPlexActor} relations were given to be inserted for library {LibraryName} with {libraryId}, all current {NameOfPlexActor2} relations will be dropped",
                    nameof(PlexActor),
                    libraryName,
                    libraryId,
                    nameof(PlexActor)
                );

            await _dbContext.PlexLibraryActors.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync(ct);
            stopWatch.Stop();

            _log.Here()
                .Debug(
                    "Finished dropping all {NameOfPlexActor} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                    nameof(PlexActor),
                    libraryName,
                    stopWatch.Elapsed.TotalSeconds
                );

            return Result.Ok(0);
        }

        // Reinsert actors for the library
        var newActors = sourceDict
            .Select(x => new PlexLibraryActors(libraryId: libraryId, plexActorId: x.Value.Id))
            .ToList();

        _log.Here().Debug("[SyncMetaData] BulkInsertAsync actors starting ({Count} rows)", newActors.Count);
        var insertResult = await _dbContext.ExecuteSerializedTransactionAsync(async (ctx, txCt) =>
        {
            await ctx.PlexLibraryActors.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync(txCt);
            await ctx.BulkInsertAsync(newActors, _bulkInsertConfig, txCt);
        }, ct);
        _log.Here().Debug("[SyncMetaData] BulkInsertAsync actors done. IsFailed={IsFailed}", insertResult.IsFailed);

        stopWatch.Stop();

        if (insertResult.IsSuccess)
        {
            _log.Here()
                .Debug(
                    "Finished creating {Count} {NameOfPlexActor} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                    newActors.Count,
                    nameof(PlexActor),
                    libraryName,
                    stopWatch.Elapsed.TotalSeconds
                );
            return Result.Ok(newActors.Count);
        }

        _log.Here()
            .Error(
                "Failed creating {Count} {NameOfPlexActor} relations for library {LibraryName} after {ElapsedSeconds:F2} seconds",
                newActors.Count,
                libraryName,
                nameof(PlexActor),
                stopWatch.Elapsed.TotalSeconds
            );
        return insertResult.LogError();
    }

    private async Task<Result<int>> SyncGenres(
        Dictionary<string, PlexGenre> sourceDict,
        int libraryId,
        string libraryName,
        CancellationToken ct
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started syncing {Count} genres for library {LibraryName}", sourceDict.Count, libraryName);

        if (!sourceDict.Any())
        {
            _log.Here()
                .Warning(
                    "No {NameOfPlexGenre} relations were given to be inserted for library {LibraryName} with {libraryId}, all current {NameOfPlexGenre2} relations will be dropped",
                    nameof(PlexGenre),
                    libraryName,
                    libraryId,
                    nameof(PlexGenre)
                );
            await _dbContext.PlexLibraryGenres.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync(ct);

            stopWatch.Stop();

            _log.Here()
                .Debug(
                    "Finished dropping all {NameOfPlexGenre} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                    nameof(PlexGenre),
                    libraryName,
                    stopWatch.Elapsed.TotalSeconds
                );
            return Result.Ok(0);
        }

        // Reinsert genres for the library
        var newGenres = sourceDict.Select(x => new PlexLibraryGenres(libraryId, x.Value.Id)).ToList();
        _log.Here().Debug("[SyncMetaData] BulkInsertAsync genres starting ({Count} rows)", newGenres.Count);
        var insertResult = await _dbContext.ExecuteSerializedTransactionAsync(async (ctx, txCt) =>
        {
            await ctx.PlexLibraryGenres.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync(txCt);
            await ctx.BulkInsertAsync(newGenres, _bulkInsertConfig, txCt);
        }, ct);
        _log.Here().Debug("[SyncMetaData] BulkInsertAsync genres done. IsFailed={IsFailed}", insertResult.IsFailed);

        stopWatch.Stop();

        if (insertResult.IsSuccess)
        {
            _log.Here()
                .Debug(
                    "Finished creating {Count} {NameOfPlexGenre} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                    newGenres.Count,
                    nameof(PlexGenre),
                    libraryName,
                    stopWatch.Elapsed.TotalSeconds
                );
            return Result.Ok(newGenres.Count);
        }

        _log.Here()
            .Error(
                "Failed creating {Count} {NameOfPlexGenre} relations for library {LibraryName} after {ElapsedSeconds:F2} seconds",
                newGenres.Count,
                libraryName,
                nameof(PlexGenre),
                stopWatch.Elapsed.TotalSeconds
            );
        return insertResult.LogError();
    }

    private async Task<Result<int>> SyncCountries(
        Dictionary<string, PlexCountry> sourceDict,
        int libraryId,
        string libraryName,
        CancellationToken ct
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started syncing {Count} countries for library {LibraryName}", sourceDict.Count, libraryName);

        if (!sourceDict.Any())
        {
            _log.Here()
                .Warning(
                    "No {NameOfPlexCountry} relations were given to be inserted for library {LibraryName} with {libraryId}, all current {NameOfPlexCountry2} relations will be dropped",
                    nameof(PlexCountry),
                    libraryName,
                    libraryId,
                    nameof(PlexCountry)
                );
            await _dbContext.PlexLibraryCountries.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync(ct);
            stopWatch.Stop();

            _log.Here()
                .Debug(
                    "Finished dropping all {NameOfPlexCountry} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                    nameof(PlexCountry),
                    libraryName,
                    stopWatch.Elapsed.TotalSeconds
                );
            return Result.Ok(0);
        }

        // Reinsert countries for the library
        var newCountries = sourceDict.Select(x => new PlexLibraryCountries(libraryId, x.Value.Id)).ToList();
        _log.Here().Debug("[SyncMetaData] BulkInsertAsync countries starting ({Count} rows)", newCountries.Count);
        var insertResult = await _dbContext.ExecuteSerializedTransactionAsync(async (ctx, txCt) =>
        {
            await ctx.PlexLibraryCountries.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync(txCt);
            await ctx.BulkInsertAsync(newCountries, _bulkInsertConfig, txCt);
        }, ct);
        _log.Here().Debug("[SyncMetaData] BulkInsertAsync countries done. IsFailed={IsFailed}", insertResult.IsFailed);

        stopWatch.Stop();

        if (insertResult.IsSuccess)
        {
            _log.Here()
                .Debug(
                    "Finished creating {Count} {NameOfPlexCountry} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                    newCountries.Count,
                    nameof(PlexCountry),
                    libraryName,
                    stopWatch.Elapsed.TotalSeconds
                );
            return Result.Ok(newCountries.Count);
        }

        _log.Here()
            .Error(
                "Failed creating {Count} {NameOfPlexCountry} relations for library {LibraryName} after {ElapsedSeconds:F2} seconds",
                newCountries.Count,
                libraryName,
                nameof(PlexCountry),
                stopWatch.Elapsed.TotalSeconds
            );
        return insertResult.LogError();
    }
}

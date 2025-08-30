using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EFCore.BulkExtensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.Logging;
using Reaparr.PlexApi.Contracts;
using ILog = Reaparr.Logging.ILog;

namespace Reaparr.Application;

/// <summary>
/// Command to insert or update media metadata (actors, genres, and countries) for a <see cref="PlexLibrary"/> into the database.
/// This operation ensures all metadata entities exist with proper database IDs, enabling media items to be linked to their associated metadata.
/// The command performs bulk upsert operations for optimal performance and returns dictionaries of inserted entities keyed by their Plex IDs.
/// </summary>
/// <param name="LibraryMetadata">The library metadata containing collections of actors, genres, and countries to be inserted or updated in the database</param>
public record InsertMediaMetaDataCommand(LibraryMetadata LibraryMetadata)
    : ICommand<Result<InsertMediaMetaDataCommandResponse>>;

public class InsertMediaMetaDataCommandValidator : Validator<InsertMediaMetaDataCommand>
{
    public InsertMediaMetaDataCommandValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
    }
}

public record InsertMediaMetaDataCommandResponse
{
    [SetsRequiredMembers]
    public InsertMediaMetaDataCommandResponse(PlexLibrary plexLibrary)
    {
        PlexLibrary = plexLibrary;
    }

    public PlexLibrary PlexLibrary { get; private set; }

    public int PlexLibraryId => PlexLibrary.Id;

    /// <summary>
    /// The string key is the hashkey of the actor name for this <see cref="PlexLibrary"/>
    /// </summary>
    public required Dictionary<string, PlexActor> PlexActors { get; init; } = new();

    /// <summary>
    /// The string key is the hashkey of the genre name for this <see cref="PlexLibrary"/>
    /// </summary>
    public required Dictionary<string, PlexGenre> PlexGenres { get; init; } = new();

    /// <summary>
    /// The string key is the hashkey of the country name for this <see cref="PlexLibrary"/>
    /// </summary>
    public required Dictionary<string, PlexCountry> PlexCountries { get; init; } = new();
}

public class InsertMediaMetaDataCommandHandler
    : ICommandHandler<InsertMediaMetaDataCommand, Result<InsertMediaMetaDataCommandResponse>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public InsertMediaMetaDataCommandHandler(IPlexRipperDbContext dbContext, ILog log)
    {
        _dbContext = dbContext;
        _log = log;
    }

    public async Task<Result<InsertMediaMetaDataCommandResponse>> ExecuteAsync(
        InsertMediaMetaDataCommand command,
        CancellationToken ct
    )
    {
        var roles = command.LibraryMetadata.Actors;
        var genres = command.LibraryMetadata.Genres;
        var countries = command.LibraryMetadata.Countries;

        var syncGenresResult = await InsertGenres(genres);
        var syncCountriesResult = await InsertCountries(countries);
        var syncRolesResult = await InsertPlexActors(roles);

        var results = Result.Merge(syncGenresResult, syncCountriesResult, syncRolesResult);
        if (results.IsFailed)
            return results;

        return Result.Ok(
            new InsertMediaMetaDataCommandResponse(command.LibraryMetadata.Library)
            {
                PlexActors = syncRolesResult.Value,
                PlexGenres = syncGenresResult.Value,
                PlexCountries = syncCountriesResult.Value,
            }
        );
    }

    private async Task<Result<Dictionary<string, PlexActor>>> InsertPlexActors(
        IReadOnlyCollection<LibraryMediaItemRoleDTO> sourceList
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started inserting {Count} {NameOfPlexActor}", sourceList.Count, nameof(PlexActor));

        var newPlexActors = sourceList
            .Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Key)
            .Select(x => x.ToPlexActor())
            .ToList();
        if (!newPlexActors.Any())
        {
            _log.Here().Debug("No {PlexActorName} to insert", nameof(PlexActor));
            return Result.Ok(new Dictionary<string, PlexActor>());
        }

        var result = await Result.Try(() =>
            _dbContext.BulkInsertOrUpdateAsync(
                newPlexActors,
                new BulkConfig
                {
                    SetOutputIdentity = false,
                    UpdateByProperties = [nameof(PlexActor.Key)],
                    UseTempDB = true,
                }
            )
        );

        if (result.IsFailed)
        {
            _log.Error(
                "Failed to insert {NameOfPlexActor} after {ElapsedSeconds:F2} seconds",
                nameof(PlexActor),
                stopWatch.Elapsed.TotalSeconds
            );
            return result.LogError();
        }

        // Query the database to get entities with proper IDs (SQLite limitation workaround)
        var newPlexActorKeys = newPlexActors.Select(x => x.Key).ToHashSet();
        newPlexActors = await _dbContext.PlexActors.Where(x => newPlexActorKeys.Contains(x.Key)).ToListAsync();

        var plexActorsWithIds = newPlexActors.ToHashKeyDictionary(sourceList);

        stopWatch.Stop();

        _log.Debug(
            "Finished inserting {Count} {NameOfPlexActor} for library {ElapsedSeconds:F2} seconds",
            sourceList.Count,
            nameof(PlexActor),
            stopWatch.Elapsed.TotalSeconds
        );
        return Result.Ok(plexActorsWithIds);
    }

    private async Task<Result<Dictionary<string, PlexGenre>>> InsertGenres(
        IReadOnlyCollection<LibraryMediaItemGenreDTO> sourceList
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started inserting {Count} {NameOfPlexGenre}", sourceList.Count, nameof(PlexGenre));

        // Distinct by Genre Name because PlexId is not globally unique across all Plex servers
        var newPlexGenres = sourceList
            .Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Key)
            .ToPlexGenre();
        if (!newPlexGenres.Any())
        {
            _log.Here().Debug("No {NameOfPlexGenre} to insert ", nameof(PlexGenre));
            return Result.Ok(new Dictionary<string, PlexGenre>());
        }

        var insertResult = await Result.Try(async Task () =>
        {
            var incomingGenreKeys = newPlexGenres.Select(x => x.Key).ToHashSet();
            var existingGenreKeys = _dbContext
                .PlexGenres.Where(c => incomingGenreKeys.Contains(c.Key))
                .Select(c => c.Key)
                .ToHashSet();

            var toInsert = newPlexGenres.Where(c => !existingGenreKeys.Contains(c.Key)).ToList();
            if (toInsert.Any())
            {
                _dbContext.PlexGenres.AddRange(toInsert);
                await _dbContext.SaveChangesAsync();
            }
        });

        if (insertResult.IsFailed)
        {
            _log.Error(
                "Failed to insert {NameOfPlexGenre} after {ElapsedSeconds:F2} seconds",
                nameof(PlexGenre),
                stopWatch.Elapsed.TotalSeconds
            );
            return insertResult.LogError();
        }

        // Query the database to get entities with proper IDs (SQLite limitation workaround)
        var newPlexGenreKeys = newPlexGenres.Select(x => x.Key).ToHashSet();
        var plexGenresWithIds = await _dbContext.PlexGenres.Where(x => newPlexGenreKeys.Contains(x.Key)).ToListAsync();
        var resultDict = plexGenresWithIds.ToHashKeyDictionary(sourceList);

        stopWatch.Stop();

        _log.Debug(
            "Finished inserting {Count} {NameOfPlexGenre} in {ElapsedSeconds:F2} seconds",
            newPlexGenres.Count,
            nameof(PlexGenre),
            stopWatch.Elapsed.TotalSeconds
        );
        return Result.Ok(resultDict);
    }

    private async Task<Result<Dictionary<string, PlexCountry>>> InsertCountries(
        IReadOnlyCollection<LibraryMediaItemCountryDTO> sourceList
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started inserting {Count} {NameOfPlexCountry}", sourceList.Count, nameof(PlexCountry));

        var newPlexCountries = sourceList
            .Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Key)
            .ToPlexCountry();
        if (!newPlexCountries.Any())
        {
            _log.Here().Debug("No {NameOfPlexCountry} to insert", nameof(PlexCountry));
            return Result.Ok(new Dictionary<string, PlexCountry>());
        }

        var insertResult = await Result.Try(async Task () =>
        {
            var incomingCountryKeys = newPlexCountries.Select(x => x.Key).ToHashSet();
            var existingCountryKeys = _dbContext
                .PlexCountries.Where(c => incomingCountryKeys.Contains(c.Key))
                .Select(c => c.Key)
                .ToHashSet();

            var toInsert = newPlexCountries.Where(c => !existingCountryKeys.Contains(c.Key)).ToList();
            if (toInsert.Any())
            {
                _dbContext.PlexCountries.AddRange(toInsert);
                await _dbContext.SaveChangesAsync();
            }
        });

        if (insertResult.IsFailed)
        {
            _log.Error(
                "Failed to insert {NameOfPlexCountry} after {ElapsedSeconds:F2} seconds",
                nameof(PlexCountry),
                stopWatch.Elapsed.TotalSeconds
            );
            return insertResult.LogError();
        }

        // Query the database to get entities with proper IDs (SQLite limitation workaround)
        var countryHashSet = newPlexCountries.Select(x => x.Key).ToHashSet();
        newPlexCountries = await _dbContext.PlexCountries.Where(x => countryHashSet.Contains(x.Key)).ToListAsync();

        var resultDict = newPlexCountries.ToHashKeyDictionary(sourceList);

        stopWatch.Stop();

        _log.Debug(
            "Finished inserting {Count} {NameOfPlexCountry} in {ElapsedSeconds:F2} seconds",
            newPlexCountries.Count,
            nameof(PlexCountry),
            stopWatch.Elapsed.TotalSeconds
        );
        return Result.Ok(resultDict);
    }
}

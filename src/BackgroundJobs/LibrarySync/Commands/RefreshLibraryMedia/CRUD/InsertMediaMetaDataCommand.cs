using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.BackgroundJobs;

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
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public InsertMediaMetaDataCommandHandler(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log.ForContext<InsertMediaMetaDataCommandHandler>();
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
        if (syncGenresResult.IsFailed)
            return syncGenresResult.ToResult();

        var syncCountriesResult = await InsertCountries(countries);
        if (syncCountriesResult.IsFailed)
            return syncCountriesResult.ToResult();

        var syncRolesResult = await InsertPlexActors(roles);
        if (syncRolesResult.IsFailed)
            return syncRolesResult.ToResult();

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

        _log.Here()
            .Debug(
                "Started inserting {RawCount} {NameOfPlexActor} (pre-filtered/grouped)",
                sourceList.Count,
                nameof(PlexActor)
            );

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

        _log.Here()
            .Debug("Inserting {Count} {NameOfPlexActor} after deduplication", newPlexActors.Count, nameof(PlexActor));

        const int chunkSize = 500;

        // Chunk the existingKeys query to avoid hitting SQLite IN-clause parameter limits
        var incomingKeys = newPlexActors.Select(x => x.Key).ToList();
        var existingIdByKey = new Dictionary<string, int>(newPlexActors.Count);
        foreach (var chunk in incomingKeys.Chunk(chunkSize))
        {
            var found = await _dbContext
                .PlexActors.AsNoTracking()
                .Where(a => chunk.Contains(a.Key))
                .Select(a => new { a.Key, a.Id })
                .ToListAsync();
            foreach (var a in found)
                existingIdByKey[a.Key] = a.Id;
        }

        _log.Here()
            .Debug(
                "{ExistingCount} {NameOfPlexActor} already exist in DB, inserting {NewCount} new",
                existingIdByKey.Count,
                nameof(PlexActor),
                newPlexActors.Count - existingIdByKey.Count
            );

        var toInsert = newPlexActors.Where(a => !existingIdByKey.ContainsKey(a.Key)).ToList();

        var insertResult = await Result.Try(async Task () =>
        {
            foreach (var chunk in toInsert.Chunk(chunkSize))
            {
                _dbContext.PlexActors.AddRange(chunk);
                await _dbContext.SaveChangesAsync();
                _dbContext.ClearChangeTracker();
            }
        });

        if (insertResult.IsFailed)
        {
            _log.Here()
                .Error(
                    "Failed to insert {NameOfPlexActor} after {ElapsedSeconds:F2} seconds",
                    nameof(PlexActor),
                    stopWatch.Elapsed.TotalSeconds
                );
            return insertResult.LogError();
        }

        // Fetch IDs for newly inserted actors and merge with already-known IDs
        foreach (var chunk in toInsert.Chunk(chunkSize))
        {
            var keys = chunk.Select(a => a.Key).ToArray();
            var found = await _dbContext
                .PlexActors.AsNoTracking()
                .Where(x => keys.Contains(x.Key))
                .Select(a => new { a.Key, a.Id })
                .ToListAsync();
            foreach (var a in found)
                existingIdByKey[a.Key] = a.Id;
        }

        // Build result dictionary from merged Id map — avoids re-fetching all actors from DB
        var result = new Dictionary<string, PlexActor>(newPlexActors.Count);
        foreach (var actor in newPlexActors)
        {
            if (!existingIdByKey.TryGetValue(actor.Key, out var id))
                continue;

            actor.Id = id;
            result[actor.Key] = actor;
        }

        stopWatch.StopAndLog($"Finished inserting {sourceList.Count} {nameof(PlexActor)} for library");

        return Result.Ok(result);
    }

    private async Task<Result<Dictionary<string, PlexGenre>>> InsertGenres(
        IReadOnlyCollection<LibraryMediaItemGenreDTO> sourceList
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here()
            .Debug(
                "Started inserting {RawCount} {NameOfPlexGenre} (pre-filtered/grouped)",
                sourceList.Count,
                nameof(PlexGenre)
            );

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

        _log.Here()
            .Debug("Inserting {Count} {NameOfPlexGenre} after deduplication", newPlexGenres.Count, nameof(PlexGenre));

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
            _log.Here()
                .Error(
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

        stopWatch.StopAndLog($"Finished inserting {newPlexGenres.Count} {nameof(PlexGenre)}");

        return Result.Ok(resultDict);
    }

    private async Task<Result<Dictionary<string, PlexCountry>>> InsertCountries(
        IReadOnlyCollection<LibraryMediaItemCountryDTO> sourceList
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here()
            .Debug(
                "Started inserting {RawCount} {NameOfPlexCountry} (pre-filtered/grouped)",
                sourceList.Count,
                nameof(PlexCountry)
            );

        var newPlexCountries = sourceList
            .Where(x => !string.IsNullOrEmpty(x.Key) && !string.IsNullOrEmpty(x.Name))
            .DistinctBy(x => x.Key)
            .ToPlexCountry();
        if (!newPlexCountries.Any())
        {
            _log.Here().Debug("No {NameOfPlexCountry} to insert", nameof(PlexCountry));
            return Result.Ok(new Dictionary<string, PlexCountry>());
        }

        _log.Here()
            .Debug(
                "Inserting {Count} {NameOfPlexCountry} after deduplication",
                newPlexCountries.Count,
                nameof(PlexCountry)
            );

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
            _log.Here()
                .Error(
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

        stopWatch.StopAndLog($"Finished inserting {newPlexCountries.Count} {nameof(PlexCountry)}");

        return Result.Ok(resultDict);
    }
}

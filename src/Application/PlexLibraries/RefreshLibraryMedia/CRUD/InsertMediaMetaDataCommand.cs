using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using Environment;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.IdentityModel.Tokens;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record InsertMediaMetaDataCommand(LibraryMetadata LibraryMetadata)
    : ICommand<Result<InsertMediaMetaDataCommandResponse>>;

public class InsertMediaMetaDataValidator : Validator<InsertMediaMetaDataCommand>
{
    public InsertMediaMetaDataValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
    }
}

public record InsertMediaMetaDataCommandResponse
{
    public required int PlexLibraryId { get; init; }

    /// <summary>
    /// The int key is the PlexId of the actor for this <see cref="PlexLibrary"/>
    /// </summary>
    public required Dictionary<int, PlexActor> PlexActors { get; init; }

    /// <summary>
    /// The int key is the PlexId of the genre for this <see cref="PlexLibrary"/>
    /// </summary>
    public required Dictionary<int, PlexGenre> PlexGenres { get; init; }

    /// <summary>
    /// The int key is the PlexId of the country for this <see cref="PlexLibrary"/>
    /// </summary>
    public required Dictionary<int, PlexCountry> PlexCountries { get; init; }
}

public class InsertMediaMetaDataCommandHandler
    : ICommandHandler<InsertMediaMetaDataCommand, Result<InsertMediaMetaDataCommandResponse>>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly BulkConfig? _bulkInsertKeyConfig =
        new()
        {
            SetOutputIdentity = false,
            UpdateByProperties = [nameof(PlexActor.Key)],

            // Only in-memory sqlite needs this which happens during testing
            UseTempDB = EnvironmentExtensions.IsIntegrationTestMode(),
        };

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
        var plexLibraryId = command.LibraryMetadata.Library.Id;
        var roles = command.LibraryMetadata.Actors;
        var genres = command.LibraryMetadata.Genres;
        var countries = command.LibraryMetadata.Countries;

        var syncGenresResult = await InsertGenres(genres);
        var syncCountriesResult = await InsertCountries(countries);
        var syncRolesResult = await InsertPlexRoles(roles);

        var results = Result.Merge(syncGenresResult, syncCountriesResult, syncRolesResult);
        if (results.IsFailed)
            return results;

        return Result.Ok(
            new InsertMediaMetaDataCommandResponse
            {
                PlexLibraryId = plexLibraryId,
                PlexActors = syncRolesResult.Value,
                PlexGenres = syncGenresResult.Value,
                PlexCountries = syncCountriesResult.Value,
            }
        );
    }

    private async Task<Result<Dictionary<int, PlexActor>>> InsertPlexRoles(
        IReadOnlyCollection<LibraryMediaItemRoleDTO> sourceList
    )
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _log.Here().Debug("Started inserting {Count} {NameOfPlexActor}", sourceList.Count, nameof(PlexActor));

        var resultDict = new Dictionary<int, PlexActor>();

        var distinctRoles = sourceList.Where(x => x.TagKey != null).DistinctBy(x => x.TagKey).ToList();
        var plexActors = distinctRoles.ToPlexActor();

        if (plexActors.IsNullOrEmpty())
        {
            _log.Here().Debug("No {PlexActorName} to insert", nameof(PlexActor));
            return Result.Ok(resultDict);
        }

        var result = await Result.Try(
            () => _dbContext.BulkInsertOrUpdateAsync(plexActors, _bulkInsertKeyConfig),
            e => new ExceptionalError(e)
        );

        resultDict = plexActors.ToPlexIdDictionary(sourceList);

        stopWatch.Stop();

        if (result.IsSuccess)
        {
            _log.Debug(
                "Finished inserting {Count} {NameOfPlexActor} for library {ElapsedSeconds:F2} seconds",
                sourceList.Count,
                nameof(PlexActor),
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(resultDict);
        }

        _log.Error(
            "Failed to insert {NameOfPlexActor} after {ElapsedSeconds:F2} seconds",
            nameof(PlexActor),
            stopWatch.Elapsed.TotalSeconds
        );
        return result.LogError();
    }

    private async Task<Result<Dictionary<int, PlexGenre>>> InsertGenres(
        IReadOnlyCollection<LibraryMediaItemGenreDTO> sourceList
    )
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _log.Here().Debug("Started inserting {Count} {NameOfPlexGenre}", sourceList.Count, nameof(PlexGenre));

        var resultDict = new Dictionary<int, PlexGenre>();

        // Distinct by Genre Name because PlexId is not globally unique across all Plex servers
        var plexGenres = sourceList.DistinctBy(x => x.Key).ToPlexGenre();
        if (plexGenres.IsNullOrEmpty())
        {
            _log.Here().Debug("No {NameOfPlexGenre} to insert ", nameof(PlexGenre));
            return Result.Ok(resultDict);
        }

        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertOrUpdateAsync(plexGenres, _bulkInsertKeyConfig),
            e => new ExceptionalError(e)
        );

        resultDict = plexGenres.ToPlexIdDictionary(sourceList);

        stopWatch.Stop();
        if (insertResult.IsSuccess)
        {
            _log.Debug(
                "Finished inserting {Count} {NameOfPlexGenre} in {ElapsedSeconds:F2} seconds",
                plexGenres.Count,
                nameof(PlexGenre),
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(resultDict);
        }

        _log.Error(
            "Failed to insert {NameOfPlexGenre} after {ElapsedSeconds:F2} seconds",
            nameof(PlexGenre),
            stopWatch.Elapsed.TotalSeconds
        );
        return insertResult.LogError();
    }

    private async Task<Result<Dictionary<int, PlexCountry>>> InsertCountries(
        IReadOnlyCollection<LibraryMediaItemCountryDTO> sourceList
    )
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _log.Here().Debug("Started inserting {Count} {NameOfPlexCountry}", sourceList.Count, nameof(PlexCountry));

        var resultDict = new Dictionary<int, PlexCountry>();

        // Distinct by Country Name because PlexId is not globally unique across all Plex servers
        var plexCountries = sourceList.DistinctBy(x => x.Name).ToPlexCountry();
        if (plexCountries.IsNullOrEmpty())
        {
            _log.Here().Debug("No {NameOfPlexCountry} to insert", nameof(PlexCountry));
            return Result.Ok(resultDict);
        }

        var result = await Result.Try(
            () => _dbContext.BulkInsertOrUpdateAsync(plexCountries, _bulkInsertKeyConfig),
            e => new ExceptionalError(e)
        );

        resultDict = plexCountries.ToPlexIdDictionary(sourceList);

        stopWatch.Stop();

        if (result.IsSuccess)
        {
            _log.Debug(
                "Finished inserting {Count} {NameOfPlexCountry} in {ElapsedSeconds:F2} seconds",
                plexCountries.Count,
                nameof(PlexCountry),
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(resultDict);
        }

        _log.Error(
            "Failed to insert {NameOfPlexCountry} after {ElapsedSeconds:F2} seconds",
            nameof(PlexCountry),
            stopWatch.Elapsed.TotalSeconds
        );
        return result.LogError();
    }
}

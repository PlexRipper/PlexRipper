using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using FastEndpoints;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace PlexRipper.Application;

public record SyncPlexLibraryMediaMetaDataCommand(InsertMediaMetaDataCommandResponse LibraryMetadata)
    : ICommand<Result>;

public class SyncPlexLibraryMediaMetaDataCommandValidator : Validator<SyncPlexLibraryMediaMetaDataCommand>
{
    public SyncPlexLibraryMediaMetaDataCommandValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.LibraryMetadata.PlexLibraryId).GreaterThan(0);
        RuleFor(x => x.LibraryMetadata.PlexActors).NotNull();
        RuleForEach(x => x.LibraryMetadata.PlexActors)
            .ChildRules(y =>
            {
                y.RuleFor(z => z.Value.Id).GreaterThan(0);
                y.RuleFor(z => z.Value.Key).NotEmpty();
            });

        RuleFor(x => x.LibraryMetadata.PlexGenres).NotNull();
        RuleForEach(x => x.LibraryMetadata.PlexGenres)
            .ChildRules(y =>
            {
                y.RuleFor(z => z.Value.Id).GreaterThan(0);
                y.RuleFor(z => z.Value.Name).NotEmpty();
                y.RuleFor(z => z.Value.Key).NotEmpty();
            });

        RuleFor(x => x.LibraryMetadata.PlexCountries).NotNull();
        RuleForEach(x => x.LibraryMetadata.PlexCountries)
            .ChildRules(y =>
            {
                y.RuleFor(z => z.Value.Id).GreaterThan(0);
                y.RuleFor(z => z.Value.Name).NotEmpty();
                y.RuleFor(z => z.Value.Key).NotEmpty();
            });
    }
}

public class SyncPlexLibraryMediaMetaDataCommandHandler : ICommandHandler<SyncPlexLibraryMediaMetaDataCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ILog _log;

    private readonly BulkConfig? _bulkInsertConfig =
        new()
        {
            SetOutputIdentity = false,
            PreserveInsertOrder = true,
            UseTempDB = true,
        };

    public SyncPlexLibraryMediaMetaDataCommandHandler(IPlexRipperDbContext dbContext, ILog log)
    {
        _dbContext = dbContext;
        _log = log;
    }

    public async Task<Result> ExecuteAsync(SyncPlexLibraryMediaMetaDataCommand command, CancellationToken ct)
    {
        var libraryId = command.LibraryMetadata.PlexLibraryId;
        var roles = command.LibraryMetadata.PlexActors;
        var genres = command.LibraryMetadata.PlexGenres;
        var countries = command.LibraryMetadata.PlexCountries;

        var libraryDb = await _dbContext
            .PlexLibraries.AsTracking()
            .FirstOrDefaultAsync(x => x.Id == libraryId, cancellationToken: ct);

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId, CancellationToken.None);

        var syncGenresResult = await SyncGenres(genres, libraryId, libraryName);
        var syncCountriesResult = await SyncCountries(countries, libraryId, libraryName);
        var syncRolesResult = await SyncRoles(roles, libraryId, libraryName);

        libraryDb.ActorsCount = syncRolesResult.ValueOrDefault;
        libraryDb.GenresCount = syncGenresResult.ValueOrDefault;
        libraryDb.CountriesCount = syncCountriesResult.ValueOrDefault;

        await _dbContext.SaveChangesAsync(CancellationToken.None);

        return Result.Merge(syncGenresResult, syncCountriesResult, syncRolesResult).ToResult();
    }

    private async Task<Result<int>> SyncRoles(
        Dictionary<string, PlexActor> sourceDict,
        int libraryId,
        string libraryName
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started syncing {Count} roles for library {LibraryName}", sourceDict.Count, libraryName);

        if (!sourceDict.Any())
        {
            _log.Here()
                .Warning(
                    "No {NameOfPlexActor} relations were given to be inserted for library {LibraryName} with {libraryId}, all current {NameOfPlexActor} relations will be dropped",
                    nameof(PlexActor),
                    libraryName,
                    libraryId,
                    nameof(PlexActor)
                );

            await _dbContext.PlexLibraryActors.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();
            stopWatch.Stop();

            _log.Debug(
                "Finished dropping all {NameOfPlexActor} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                nameof(PlexActor),
                libraryName,
                stopWatch.Elapsed.TotalSeconds
            );

            return Result.Ok(0);
        }

        // Drop all actors for the library
        await _dbContext.PlexLibraryActors.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

        // Reinsert genres for the library
        var newActors = sourceDict
            .Select(x => new PlexLibraryActors(libraryId: libraryId, plexActorId: x.Value.Id))
            .ToList();

        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertAsync(newActors, _bulkInsertConfig),
            e => new ExceptionalError(e)
        );

        stopWatch.Stop();

        if (insertResult.IsSuccess)
        {
            _log.Debug(
                "Finished creating {Count} {NameOfPlexActor} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                newActors.Count,
                nameof(PlexActor),
                libraryName,
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(newActors.Count);
        }

        _log.Error(
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
        string libraryName
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started syncing {Count} genres for library {LibraryName}", sourceDict.Count, libraryName);

        if (!sourceDict.Any())
        {
            _log.Here()
                .Warning(
                    "No {NameOfPlexGenre} relations were given to be inserted for library {LibraryName} with {libraryId}, all current {NameOfPlexGenre} relations will be dropped",
                    nameof(PlexGenre),
                    libraryName,
                    libraryId,
                    nameof(PlexGenre)
                );
            await _dbContext.PlexLibraryGenres.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

            stopWatch.Stop();

            _log.Debug(
                "Finished dropping all {NameOfPlexGenre} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                nameof(PlexGenre),
                libraryName,
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(0);
        }

        // Drop all genres for the library
        await _dbContext.PlexLibraryGenres.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

        // Reinsert genres for the library
        var newGenres = sourceDict.Select(x => new PlexLibraryGenres(libraryId, x.Value.Id)).ToList();
        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertAsync(newGenres, _bulkInsertConfig),
            e => new ExceptionalError(e)
        );

        stopWatch.Stop();

        if (insertResult.IsSuccess)
        {
            _log.Debug(
                "Finished creating {Count} {NameOfPlexGenre} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                newGenres.Count,
                nameof(PlexGenre),
                libraryName,
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(newGenres.Count);
        }

        _log.Error(
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
        string libraryName
    )
    {
        var stopWatch = Stopwatch.StartNew();

        _log.Here().Debug("Started syncing {Count} countries for library {LibraryName}", sourceDict.Count, libraryName);

        if (!sourceDict.Any())
        {
            _log.Here()
                .Warning(
                    "No {NameOfPlexCountry} relations were given to be inserted for library {LibraryName} with {libraryId}, all current {NameOfPlexCountry} relations will be dropped",
                    nameof(PlexCountry),
                    libraryName,
                    libraryId,
                    nameof(PlexCountry)
                );
            await _dbContext.PlexLibraryCountries.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();
            stopWatch.Stop();

            _log.Debug(
                "Finished dropping all {NameOfPlexCountry} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                nameof(PlexCountry),
                libraryName,
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(0);
        }

        // Drop all countries for the library
        await _dbContext.PlexLibraryCountries.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

        // Reinsert countries for the library
        var newCountries = sourceDict.Select(x => new PlexLibraryCountries(libraryId, x.Value.Id)).ToList();
        var insertResult = await Result.Try(
            () => _dbContext.BulkInsertAsync(newCountries, _bulkInsertConfig),
            e => new ExceptionalError(e)
        );

        stopWatch.Stop();

        if (insertResult.IsSuccess)
        {
            _log.Debug(
                "Finished creating {Count} {NameOfPlexCountry} relations for library {LibraryName} in {ElapsedSeconds:F2} seconds",
                newCountries.Count,
                nameof(PlexCountry),
                libraryName,
                stopWatch.Elapsed.TotalSeconds
            );
            return Result.Ok(newCountries.Count);
        }

        _log.Error(
            "Failed creating {Count} {NameOfPlexCountry} relations for library {LibraryName} after {ElapsedSeconds:F2} seconds",
            newCountries.Count,
            libraryName,
            nameof(PlexCountry),
            stopWatch.Elapsed.TotalSeconds
        );
        return insertResult.LogError();
    }
}

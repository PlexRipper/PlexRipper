using System.Diagnostics;
using Data.Contracts;
using EFCore.BulkExtensions;
using Environment;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application;

public record SyncPlexLibraryMediaMetaDataCommand(LibraryMetadata LibraryMetadata, int PlexLibraryId)
    : IRequest<Result>;

public class SyncPlexLibraryMediaMetaDataCommandValidator : AbstractValidator<SyncPlexLibraryMediaMetaDataCommand>
{
    public SyncPlexLibraryMediaMetaDataCommandValidator()
    {
        RuleFor(x => x.LibraryMetadata).NotNull();
        RuleFor(x => x.PlexLibraryId).GreaterThan(0);
    }
}

public class SyncPlexLibraryMediaMetaDataCommandHandler : IRequestHandler<SyncPlexLibraryMediaMetaDataCommand, Result>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly ILog _log;

    private readonly BulkConfig? _bulkInsertConfig =
        new()
        {
            SetOutputIdentity = true,
            SetOutputNonIdentityColumns = true,
            PreserveInsertOrder = true,
            // Only in-memory sqlite needs this which happens during testing
            UseTempDB = EnvironmentExtensions.IsIntegrationTestMode(),
        };

    private readonly BulkConfig? _bulkReadConfig = new() { UpdateByProperties = ["PlexKey"] };

    public SyncPlexLibraryMediaMetaDataCommandHandler(IPlexRipperDbContext dbContext, ILog log)
    {
        _dbContext = dbContext;
        _log = log;
    }

    public async Task<Result> Handle(SyncPlexLibraryMediaMetaDataCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var libraryId = command.PlexLibraryId;
            var roles = command.LibraryMetadata.Roles;
            var genres = command.LibraryMetadata.Genres;
            var countries = command.LibraryMetadata.Countries;

            var libraryDb = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(x => x.Id == libraryId)
                .FirstOrDefaultAsync(cancellationToken);

            if (libraryDb is null)
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

            var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId, CancellationToken.None);

            await SyncRoles(roles, libraryId, libraryName);
            await SyncGenres(genres, libraryId, libraryName);
            await SyncCountries(countries, libraryId, libraryName);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private async Task<Result> SyncRoles(List<PlexRole> roles, int libraryId, string libraryName)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _log.Here().Debug("Started syncing {Count} roles for library {LibraryName}", roles.Count, libraryName);

        var distinctRoles = roles.DistinctBy(x => x.PlexKey).ToList();
        await _dbContext.BulkInsertOrUpdateAsync(distinctRoles, _bulkInsertConfig);
        await _dbContext.BulkReadAsync(distinctRoles, _bulkReadConfig);

        // Drop all roles for the library
        await _dbContext.PlexLibraryRoles.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

        // Reinsert roles for the library
        var connections = distinctRoles.Select(x => new PlexLibraryRoles(libraryId, x.Id)).ToList();
        await _dbContext.BulkInsertAsync(connections, _bulkInsertConfig);

        stopWatch.Stop();
        _log.Debug(
            "Finished syncing {Count} roles for library {LibraryName} in {ElapsedSeconds:F2} seconds",
            roles.Count,
            libraryName,
            stopWatch.Elapsed.TotalSeconds
        );

        return Result.Ok();
    }

    private async Task<Result> SyncGenres(List<PlexGenre> genres, int libraryId, string libraryName)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        _log.Here().Debug("Started syncing {Count} genres for library {LibraryName}", genres.Count, libraryName);

        var distinctGenres = genres.DistinctBy(x => x.PlexKey).ToList();
        await _dbContext.BulkInsertOrUpdateAsync(distinctGenres, _bulkInsertConfig);
        await _dbContext.BulkReadAsync(distinctGenres, _bulkReadConfig);

        // Drop all genres for the library
        await _dbContext.PlexLibraryGenres.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

        // Reinsert genres for the library
        var connections = distinctGenres.Select(x => new PlexLibraryGenres(libraryId, x.Id)).ToList();
        await _dbContext.BulkInsertAsync(connections, _bulkInsertConfig);

        stopWatch.Stop();
        _log.Debug(
            "Finished syncing {Count} genres for library {LibraryName} in {ElapsedSeconds:F2} seconds",
            genres.Count,
            libraryName,
            stopWatch.Elapsed.TotalSeconds
        );

        return Result.Ok();
    }

    private async Task<Result> SyncCountries(List<PlexCountry> countries, int libraryId, string libraryName)
    {
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        _log.Here().Debug("Started syncing {Count} countries for library {LibraryName}", countries.Count, libraryName);

        var distinctCountries = countries.DistinctBy(x => x.PlexKey).ToList();
        await _dbContext.BulkInsertOrUpdateAsync(distinctCountries, _bulkInsertConfig);
        await _dbContext.BulkReadAsync(distinctCountries, _bulkReadConfig);

        // Drop all countries for the library
        await _dbContext.PlexLibraryCountries.Where(x => x.PlexLibraryId == libraryId).ExecuteDeleteAsync();

        // Reinsert countries for the library
        var connections = distinctCountries.Select(x => new PlexLibraryCountries(libraryId, x.Id)).ToList();
        await _dbContext.BulkInsertAsync(connections, _bulkInsertConfig);

        stopWatch.Stop();
        _log.Debug(
            "Finished syncing {Count} countries for library {LibraryName} in {ElapsedSeconds:F2} seconds",
            countries.Count,
            libraryName,
            stopWatch.Elapsed.TotalSeconds
        );

        return Result.Ok();
    }
}

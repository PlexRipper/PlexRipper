using System.Diagnostics;
using Data.Contracts;
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
                .PlexLibraries.Where(x => x.Id == libraryId)
                .FirstOrDefaultAsync(cancellationToken);

            var libraryName = await _dbContext.GetPlexLibraryNameById(libraryId, CancellationToken.None);

            if (libraryDb is null)
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

            await Task.WhenAll(
                SyncRoles(roles, libraryId, libraryName),
                SyncGenres(genres, libraryId, libraryName),
                SyncCountries(countries, libraryId, libraryName)
            );

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

        foreach (var plexRole in roles)
            _dbContext.PlexRoles.AddIfNotExists(plexRole, x => x.PlexKey == plexRole.PlexKey);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Roles)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var roleKeys = roles.Select(x => x.PlexKey).ToHashSet();

        var rolesDb = await _dbContext
            .PlexRoles.Where(x => roleKeys.Contains(x.PlexKey))
            .AsTracking()
            .Take(roleKeys.Count)
            .ToListAsync();

        if (libraryDb.Roles.Any())
        {
            foreach (var role in libraryDb.Roles.Where(r => !roleKeys.Contains(r.PlexKey)).ToList())
                libraryDb.Roles.Remove(role);
        }

        // Add Roles
        var currentKeys = libraryDb.Roles.Select(x => x.PlexKey).ToList();
        var rolesToAdd = rolesDb.Where(x => !currentKeys.Contains(x.PlexKey)).ToList();
        libraryDb.Roles.AddRange(rolesToAdd);

        await _dbContext.SaveChangesAsync();

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

        foreach (var genre in genres)
            _dbContext.PlexGenres.AddIfNotExists(genre, x => x.PlexKey == genre.PlexKey);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Genres)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var roleKeys = genres.Select(x => x.PlexKey).ToHashSet();

        var genresDb = await _dbContext
            .PlexGenres.Where(x => roleKeys.Contains(x.PlexKey))
            .AsTracking()
            .Take(roleKeys.Count)
            .ToListAsync();

        if (libraryDb.Genres.Any())
        {
            foreach (var genre in libraryDb.Genres.Where(g => !roleKeys.Contains(g.PlexKey)).ToList())
                libraryDb.Genres.Remove(genre);
        }

        // Add Genres
        var currentNames = libraryDb.Genres.Select(x => x.PlexKey).ToList();
        var genresToAdd = genresDb.Where(x => !currentNames.Contains(x.PlexKey)).ToList();
        libraryDb.Genres.AddRange(genresToAdd);

        await _dbContext.SaveChangesAsync();

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

        foreach (var plexRole in countries)
            _dbContext.PlexCountries.AddIfNotExists(plexRole, x => x.PlexKey == plexRole.PlexKey);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Countries)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var countryNames = countries.Select(x => x.PlexKey).ToHashSet();

        var countriesDb = await _dbContext
            .PlexCountries.Where(x => countryNames.Contains(x.PlexKey))
            .AsTracking()
            .Take(countryNames.Count)
            .ToListAsync();

        if (libraryDb.Countries.Any())
        {
            foreach (var country in libraryDb.Countries.Where(c => !countryNames.Contains(c.PlexKey)).ToList())
                libraryDb.Countries.Remove(country);
        }

        // Add Countries
        var currentNames = libraryDb.Countries.Select(x => x.PlexKey).ToList();
        var countriesToAdd = countriesDb.Where(x => !currentNames.Contains(x.PlexKey)).ToList();
        libraryDb.Countries.AddRange(countriesToAdd);

        await _dbContext.SaveChangesAsync();

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

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
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public SyncPlexLibraryMediaMetaDataCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
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

            if (libraryDb is null)
                return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

            await SyncRoles(roles, libraryId);

            await SyncGenres(genres, libraryId);

            await SyncCountries(countries, libraryId);

            return Result.Ok();
        }
        catch (Exception e)
        {
            return Result.Fail(new ExceptionalError(e)).LogError();
        }
    }

    private async Task<Result> SyncRoles(List<PlexRole> roles, int libraryId)
    {
        foreach (var plexRole in roles)
            _dbContext.PlexRoles.AddIfNotExists(plexRole, x => x.Name == plexRole.Name);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Roles)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var roleKeys = roles.Select(x => x.Name).ToHashSet();

        var rolesDb = await _dbContext
            .PlexRoles.Where(x => roleKeys.Contains(x.Name))
            .AsTracking()
            .Take(roleKeys.Count)
            .ToListAsync();

        if (libraryDb.Roles.Any())
        {
            for (var i = libraryDb.Roles.Count - 1; i >= 0; i--)
            {
                // Already exists
                if (roleKeys.Contains(libraryDb.Roles[i].Name))
                {
                    continue;
                }

                // Delete
                if (!roleKeys.Contains(libraryDb.Roles[i].Name))
                {
                    libraryDb.Roles.RemoveAt(i);
                }
            }
        }

        // Add Roles
        var currentKeys = libraryDb.Roles.Select(x => x.Name).ToList();
        var rolesToAdd = rolesDb.Where(x => !currentKeys.Contains(x.Name)).ToList();
        libraryDb.Roles.AddRange(rolesToAdd);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    private async Task<Result> SyncGenres(List<PlexGenre> roles, int libraryId)
    {
        foreach (var plexRole in roles)
            _dbContext.PlexGenres.AddIfNotExists(plexRole, x => x.PlexKey == plexRole.PlexKey);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Genres)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var roleKeys = roles.Select(x => x.PlexKey).ToHashSet();

        var genresDb = await _dbContext
            .PlexGenres.Where(x => roleKeys.Contains(x.PlexKey))
            .AsTracking()
            .Take(roleKeys.Count)
            .ToListAsync();

        if (libraryDb.Genres.Any())
        {
            for (var i = libraryDb.Genres.Count - 1; i >= 0; i--)
            {
                // Already exists
                if (roleKeys.Contains(libraryDb.Genres[i].PlexKey))
                {
                    continue;
                }

                // Delete
                if (!roleKeys.Contains(libraryDb.Genres[i].PlexKey))
                {
                    libraryDb.Genres.RemoveAt(i);
                }
            }
        }

        // Add Genres
        var currentKeys = libraryDb.Genres.Select(x => x.PlexKey).ToList();
        var genresToAdd = genresDb.Where(x => !currentKeys.Contains(x.PlexKey)).ToList();
        libraryDb.Genres.AddRange(genresToAdd);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    private async Task<Result> SyncCountries(List<PlexCountry> countries, int libraryId)
    {
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

        var countryKeys = countries.Select(x => x.PlexKey).ToHashSet();

        var countriesDb = await _dbContext
            .PlexCountries.Where(x => countryKeys.Contains(x.PlexKey))
            .AsTracking()
            .Take(countryKeys.Count)
            .ToListAsync();

        if (libraryDb.Countries.Any())
        {
            for (var i = libraryDb.Countries.Count - 1; i >= 0; i--)
            {
                // Already exists
                if (countryKeys.Contains(libraryDb.Countries[i].PlexKey))
                {
                    continue;
                }

                // Delete
                if (!countryKeys.Contains(libraryDb.Countries[i].PlexKey))
                {
                    libraryDb.Countries.RemoveAt(i);
                }
            }
        }

        // Add Countries
        var currentKeys = libraryDb.Countries.Select(x => x.PlexKey).ToList();
        var countriesToAdd = countriesDb.Where(x => !currentKeys.Contains(x.PlexKey)).ToList();
        libraryDb.Countries.AddRange(countriesToAdd);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }
}

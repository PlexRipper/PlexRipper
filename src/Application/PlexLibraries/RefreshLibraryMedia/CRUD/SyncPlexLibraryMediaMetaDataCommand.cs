using Data.Contracts;
using FluentValidation;
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

    public SyncPlexLibraryMediaMetaDataCommandHandler(IPlexRipperDbContext dbContext)
    {
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

            await Task.WhenAll(
                SyncRoles(roles, libraryId),
                SyncGenres(genres, libraryId),
                SyncCountries(countries, libraryId)
            );

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
            foreach (var role in libraryDb.Roles.Where(r => !roleKeys.Contains(r.Name)).ToList())
                libraryDb.Roles.Remove(role);
        }

        // Add Roles
        var currentKeys = libraryDb.Roles.Select(x => x.Name).ToList();
        var rolesToAdd = rolesDb.Where(x => !currentKeys.Contains(x.Name)).ToList();
        libraryDb.Roles.AddRange(rolesToAdd);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    private async Task<Result> SyncGenres(List<PlexGenre> genres, int libraryId)
    {
        foreach (var genre in genres)
            _dbContext.PlexGenres.AddIfNotExists(genre, x => x.Name == genre.Name);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Genres)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var roleKeys = genres.Select(x => x.Name).ToHashSet();

        var genresDb = await _dbContext
            .PlexGenres.Where(x => roleKeys.Contains(x.Name))
            .AsTracking()
            .Take(roleKeys.Count)
            .ToListAsync();

        if (libraryDb.Genres.Any())
        {
            foreach (var genre in libraryDb.Genres.Where(g => !roleKeys.Contains(g.Name)).ToList())
                libraryDb.Genres.Remove(genre);
        }

        // Add Genres
        var currentNames = libraryDb.Genres.Select(x => x.Name).ToList();
        var genresToAdd = genresDb.Where(x => !currentNames.Contains(x.Name)).ToList();
        libraryDb.Genres.AddRange(genresToAdd);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }

    private async Task<Result> SyncCountries(List<PlexCountry> countries, int libraryId)
    {
        foreach (var plexRole in countries)
            _dbContext.PlexCountries.AddIfNotExists(plexRole, x => x.Name == plexRole.Name);

        await _dbContext.SaveChangesAsync();

        var libraryDb = await _dbContext
            .PlexLibraries.Where(x => x.Id == libraryId)
            .Include(x => x.Countries)
            .AsTracking()
            .FirstOrDefaultAsync();

        if (libraryDb is null)
            return ResultExtensions.EntityNotFound(nameof(PlexLibrary), libraryId);

        var countryNames = countries.Select(x => x.Name).ToHashSet();

        var countriesDb = await _dbContext
            .PlexCountries.Where(x => countryNames.Contains(x.Name))
            .AsTracking()
            .Take(countryNames.Count)
            .ToListAsync();

        if (libraryDb.Countries.Any())
        {
            foreach (var country in libraryDb.Countries.Where(c => !countryNames.Contains(c.Name)).ToList())
                libraryDb.Countries.Remove(country);
        }

        // Add Countries
        var currentNames = libraryDb.Countries.Select(x => x.Name).ToList();
        var countriesToAdd = countriesDb.Where(x => !currentNames.Contains(x.Name)).ToList();
        libraryDb.Countries.AddRange(countriesToAdd);

        await _dbContext.SaveChangesAsync();

        return Result.Ok();
    }
}

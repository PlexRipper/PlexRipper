using Microsoft.EntityFrameworkCore;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class SyncPlexLibraryMediaMetaDataCommandHandlerUnitTests
    : BaseCommandUnitTest<SyncPlexLibraryMediaMetaDataCommand, SyncPlexLibraryMediaMetaDataCommandHandler>
{
    public SyncPlexLibraryMediaMetaDataCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldSyncAllRolesGenresCountries_WhenNoneExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();
        var roles = FakeData.GetPlexRoles(seed).Generate(100);
        var genres = FakeData.GetPlexGenres(seed).Generate(100);
        var countries = FakeData.GetPlexCountries(seed).Generate(100);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles,
                Genres = genres,
                Countries = countries,
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.Count.ShouldBe(roles.Count);
        foreach (var role in roles)
            rolesDb.ShouldContain(x => x.Name == role.Name && x.PlexKey == role.PlexKey);

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.Count.ShouldBe(genres.Count);
        foreach (var genre in genres)
            genresDb.ShouldContain(x => x.Name == genre.Name && x.PlexKey == genre.PlexKey);

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.Count.ShouldBe(countries.Count);
        foreach (var country in countries)
            countriesDb.ShouldContain(x => x.Name == country.Name && x.PlexKey == country.PlexKey);
    }

    [Fact]
    public async Task ShouldUpdateExistingRolesGenresCountries_WhenSomeExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create initial data
        var initialRoles = FakeData.GetPlexRoles(seed).Generate(50);
        var initialGenres = FakeData.GetPlexGenres(seed).Generate(50);
        var initialCountries = FakeData.GetPlexCountries(seed).Generate(50);

        // Insert initial data
        await IDbContext.PlexRoles.AddRangeAsync(initialRoles);
        await IDbContext.PlexGenres.AddRangeAsync(initialGenres);
        await IDbContext.PlexCountries.AddRangeAsync(initialCountries);
        await IDbContext.SaveChangesAsync();

        // Create new data with some overlaps
        var newRoles = FakeData.GetPlexRoles(seed).Generate(100);
        var newGenres = FakeData.GetPlexGenres(seed).Generate(100);
        var newCountries = FakeData.GetPlexCountries(seed).Generate(100);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = newRoles,
                Genres = newGenres,
                Countries = newCountries,
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.Count.ShouldBe(newRoles.Count);
        foreach (var role in newRoles)
            rolesDb.ShouldContain(x => x.Name == role.Name && x.PlexKey == role.PlexKey);

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.Count.ShouldBe(newGenres.Count);
        foreach (var genre in newGenres)
            genresDb.ShouldContain(x => x.Name == genre.Name && x.PlexKey == genre.PlexKey);

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.Count.ShouldBe(newCountries.Count);
        foreach (var country in newCountries)
            countriesDb.ShouldContain(x => x.Name == country.Name && x.PlexKey == country.PlexKey);
    }

    [Fact]
    public async Task ShouldHandleEmptyLists_WhenNoDataProvided()
    {
        // Arrange
        await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = [],
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.ShouldBeEmpty();

        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        genresDb.ShouldBeEmpty();

        var countriesDb = await IDbContext.PlexCountries.ToListAsync();
        countriesDb.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenLibraryDoesNotExist()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var roles = FakeData.GetPlexRoles(seed).Generate(10);
        var genres = FakeData.GetPlexGenres(seed).Generate(10);
        var countries = FakeData.GetPlexCountries(seed).Generate(10);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles,
                Genres = genres,
                Countries = countries,
                Library = null!,
            },
            PlexLibraryId: 999999 // Non-existent ID
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRemoveOldConnections_WhenNewDataProvided()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create and insert initial data with connections
        var initialRoles = FakeData.GetPlexRoles(seed).Generate(50);
        await IDbContext.PlexRoles.AddRangeAsync(initialRoles);
        await IDbContext.SaveChangesAsync();

        var initialConnections = initialRoles.Select(x => new PlexLibraryRoles(plexLibrary.Id, x.Id)).ToList();
        await IDbContext.PlexLibraryRoles.AddRangeAsync(initialConnections);
        await IDbContext.SaveChangesAsync();

        // Create new data with different roles
        var newRoles = FakeData.GetPlexRoles(seed).Generate(30);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = newRoles,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        var connectionsDb = await IDbContext
            .PlexLibraryRoles.Where(x => x.PlexLibraryId == plexLibrary.Id)
            .ToListAsync();

        connectionsDb.Count.ShouldBe(newRoles.Count);
        foreach (var role in newRoles)
        {
            var roleDb = await IDbContext.PlexRoles.FirstOrDefaultAsync(x => x.PlexKey == role.PlexKey);
            roleDb.ShouldNotBeNull();
            connectionsDb.ShouldContain(x => x.PlexRoleId == roleDb.Id);
        }
    }

    [Fact]
    public async Task ShouldHandleDuplicatePlexKeys_WhenDataContainsDuplicates()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create data with duplicate PlexKeys
        var roles = FakeData.GetPlexRoles(seed).Generate(10);
        var duplicateRoles = roles
            .Select(r => new PlexActor
            {
                Name = r.Name + "_duplicate",
                PlexKey = r.PlexKey,
                Role = r.Role,
                TagKey = r.TagKey,
                Thumb = r.Thumb,
            })
            .ToList();
        roles.AddRange(duplicateRoles);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.Count.ShouldBe(10); // Should only have unique PlexKeys
        rolesDb.Select(x => x.PlexKey).Distinct().Count().ShouldBe(10);
    }

    [Fact]
    public async Task ShouldHandleConcurrentSyncs_WhenMultipleOperationsRun()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        var roles1 = FakeData.GetPlexRoles(seed).Generate(50);
        var roles2 = FakeData.GetPlexRoles(seed).Generate(50);

        // Act
        var command1 = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles1,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );

        var command2 = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles2,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );

        var results = await Task.WhenAll(TestHandlerExecuteAsync(command1), TestHandlerExecuteAsync(command2));

        // Assert
        results.All(x => x.IsSuccess).ShouldBeTrue();
        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.Count.ShouldBe(100);
        rolesDb.Select(x => x.PlexKey).Distinct().Count().ShouldBe(100);
    }

    [Fact]
    public async Task ShouldHandleLargeDataSet_WhenManyItemsProvided()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create a large dataset
        var roles = FakeData.GetPlexRoles(seed).Generate(1000);
        var genres = FakeData.GetPlexGenres(seed).Generate(1000);
        var countries = FakeData.GetPlexCountries(seed).Generate(1000);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = roles,
                Genres = genres,
                Countries = countries,
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        var genresDb = await IDbContext.PlexGenres.ToListAsync();
        var countriesDb = await IDbContext.PlexCountries.ToListAsync();

        rolesDb.Count.ShouldBe(1000);
        genresDb.Count.ShouldBe(1000);
        countriesDb.Count.ShouldBe(1000);
    }

    [Fact]
    public async Task ShouldPreserveDataIntegrity_WhenUpdatingExistingItems()
    {
        // Arrange
        var seed = await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create initial data
        var initialRoles = FakeData.GetPlexRoles(seed).Generate(50);
        await IDbContext.PlexRoles.AddRangeAsync(initialRoles);
        await IDbContext.SaveChangesAsync();

        // Create new data with some overlapping PlexKeys but different names
        var newRoles = initialRoles
            .Take(25)
            .Select(r => new PlexActor
            {
                PlexKey = r.PlexKey,
                Name = r.Name + "_updated",
                Role = r.Role,
                TagKey = r.TagKey,
                Thumb = r.Thumb,
            })
            .ToList();
        newRoles.AddRange(FakeData.GetPlexRoles(seed).Generate(25));

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new LibraryMetadata
            {
                Roles = newRoles,
                Genres = [],
                Countries = [],
                Library = plexLibrary,
            },
            PlexLibraryId: plexLibrary.Id
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var rolesDb = await IDbContext.PlexRoles.ToListAsync();
        rolesDb.Count.ShouldBe(50);

        // Verify that existing items were updated
        foreach (var role in newRoles.Take(25))
        {
            var dbRole = rolesDb.FirstOrDefault(x => x.PlexKey == role.PlexKey);
            dbRole.ShouldNotBeNull();
            dbRole.Name.ShouldBe(role.Name);
        }
    }

    [Fact]
    public async Task ShouldHandleNullLibraryMetadata_WhenProvided()
    {
        // Arrange
        await SetupDatabase(
            1223,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(LibraryMetadata: null!, PlexLibraryId: plexLibrary.Id);
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Has400BadRequestError().ShouldBeTrue();
    }
}

using Microsoft.EntityFrameworkCore;
using Reaparr.BaseTests;

namespace Reaparr.Application.UnitTests;

public class SyncPlexLibraryMediaMetaDataCommandUnitTests : BaseCommandUnitTest<SyncPlexLibraryMediaMetaDataCommand>
{
    public SyncPlexLibraryMediaMetaDataCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnNotFound_WhenLibraryDoesNotExist()
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

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        plexLibrary.Id = 9999; // Set to a non-existent library ID

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new InsertMediaMetaDataCommandResponse(plexLibrary)
        );

        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.Has404NotFoundError().ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldRemoveOldPlexLibraryActors_WhenNewDataIsProvided()
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

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        // Create and insert initial Plex actors
        var mediaItemActorRoles = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(50, x => x.Key);
        var plexActors = mediaItemActorRoles.ToPlexActor();

        var dbContext = IDbContext;
        await dbContext.PlexActors.AddRangeAsync(plexActors, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        var plexActorDict = plexActors.ToHashKeyDictionary(mediaItemActorRoles);

        // Create initial PlexLibraryRoles
        var initialPlexLibraryActors = plexActorDict
            .Select(x => new PlexLibraryActors(plexLibrary.Id, x.Value.Id))
            .ToList();
        dbContext = IDbContext;
        await dbContext.PlexLibraryActors.AddRangeAsync(initialPlexLibraryActors, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create new data with different roles
        var newPlexApiActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(30, x => x.Key);
        var newPlexActors = newPlexApiActors.ToPlexActor();
        await dbContext.PlexActors.AddRangeAsync(newPlexActors, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new InsertMediaMetaDataCommandResponse(plexLibrary)
            {
                PlexActors = newPlexActors.ToHashKeyDictionary(newPlexApiActors),
            }
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        dbContext = IDbContext;
        var libraryRolesList = await dbContext
            .PlexLibraryActors.Where(x => x.PlexLibraryId == plexLibrary.Id)
            .ToListAsync(CancellationToken);

        libraryRolesList.Count.ShouldBe(newPlexApiActors.Count);
        foreach (var role in newPlexApiActors)
        {
            var roleDb = await dbContext.PlexActors.FirstOrDefaultAsync(x => x.Key == role.Key, CancellationToken);
            roleDb.ShouldNotBeNull();
            libraryRolesList.ShouldContain(x => x.PlexActorId == roleDb.Id);
        }
    }

    [Fact]
    public async Task ShouldUpdatePlexLibraryCountsCorrectly_WhenSyncingMetadata()
    {
        // Arrange
        var seed = await SetupDatabase(
            1224,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        // Create actors data
        var actorRoles = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(15, x => x.Key);
        var plexActors = actorRoles.ToPlexActor();
        var dbContext = IDbContext;
        await dbContext.PlexActors.AddRangeAsync(plexActors, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create genres data
        var genreItems = FakePlexApiData.GetLibraryMediaItemGenreDTO(seed).GenerateUnique(8, x => x.Key);
        var plexGenres = genreItems.ToPlexGenre();
        await dbContext.PlexGenres.AddRangeAsync(plexGenres, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create countries data
        var countryItems = FakePlexApiData.GetLibraryMediaItemCountryDTO(seed).GenerateUnique(5, x => x.Key);
        var plexCountries = countryItems.ToPlexCountry();
        await dbContext.PlexCountries.AddRangeAsync(plexCountries, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Verify initial counts are 0
        plexLibrary.ActorsCount.ShouldBe(0);
        plexLibrary.GenresCount.ShouldBe(0);
        plexLibrary.CountriesCount.ShouldBe(0);

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new InsertMediaMetaDataCommandResponse(plexLibrary)
            {
                PlexActors = plexActors.ToHashKeyDictionary(actorRoles),
                PlexGenres = plexGenres.ToHashKeyDictionary(genreItems),
                PlexCountries = plexCountries.ToHashKeyDictionary(countryItems),
            }
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Refresh the library from the database to get updated counts
        dbContext = IDbContext;
        var updatedLibrary = await dbContext.PlexLibraries.FirstOrDefaultAsync(
            x => x.Id == plexLibrary.Id,
            CancellationToken
        );
        updatedLibrary.ShouldNotBeNull();

        // Verify the counts are updated correctly
        updatedLibrary.ActorsCount.ShouldBe(15);
        updatedLibrary.GenresCount.ShouldBe(8);
        updatedLibrary.CountriesCount.ShouldBe(5);

        // Verify the relationship entities were created correctly
        var libraryActorsCount = await dbContext.PlexLibraryActors.CountAsync(
            x => x.PlexLibraryId == plexLibrary.Id,
            CancellationToken
        );
        var libraryGenresCount = await dbContext.PlexLibraryGenres.CountAsync(
            x => x.PlexLibraryId == plexLibrary.Id,
            CancellationToken
        );
        var libraryCountriesCount = await dbContext.PlexLibraryCountries.CountAsync(
            x => x.PlexLibraryId == plexLibrary.Id,
            CancellationToken
        );

        libraryActorsCount.ShouldBe(15);
        libraryGenresCount.ShouldBe(8);
        libraryCountriesCount.ShouldBe(5);
    }

    [Fact]
    public async Task ShouldSetCountsToZero_WhenSyncingWithEmptyMetadata()
    {
        // Arrange
        var seed = await SetupDatabase(
            1225,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync(CancellationToken);
        plexLibrary.ShouldNotBeNull();

        // Create some initial data to ensure we're actually clearing it
        var actorRoles = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(5, x => x.Key);
        var plexActors = actorRoles.ToPlexActor();
        var dbContext = IDbContext;
        await dbContext.PlexActors.AddRangeAsync(plexActors, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Create initial relationship data
        var initialPlexLibraryActors = plexActors
            .ToHashKeyDictionary(actorRoles)
            .Select((x) => new PlexLibraryActors(plexLibrary.Id, x.Value.Id))
            .ToList();
        await dbContext.PlexLibraryActors.AddRangeAsync(initialPlexLibraryActors, CancellationToken);
        await dbContext.SaveChangesAsync(CancellationToken);

        // Set initial counts manually to simulate existing data
        await dbContext
            .PlexLibraries.Where(x => x.Id == plexLibrary.Id)
            .ExecuteUpdateAsync(
                p =>
                    p.SetProperty(x => x.ActorsCount, 5)
                        .SetProperty(x => x.GenresCount, 3)
                        .SetProperty(x => x.CountriesCount, 2),
                CancellationToken
            );

        // Act - sync with empty metadata
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new InsertMediaMetaDataCommandResponse(plexLibrary)
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        // Refresh the library from the database to get updated counts
        dbContext = IDbContext;
        var updatedLibrary = await dbContext.PlexLibraries.FirstOrDefaultAsync(
            x => x.Id == plexLibrary.Id,
            CancellationToken
        );
        updatedLibrary.ShouldNotBeNull();

        // Verify all counts are now 0
        updatedLibrary.ActorsCount.ShouldBe(0);
        updatedLibrary.GenresCount.ShouldBe(0);
        updatedLibrary.CountriesCount.ShouldBe(0);

        // Verify the relationship entities were actually removed
        var libraryActorsCount = await dbContext.PlexLibraryActors.CountAsync(
            x => x.PlexLibraryId == plexLibrary.Id,
            CancellationToken
        );
        var libraryGenresCount = await dbContext.PlexLibraryGenres.CountAsync(
            x => x.PlexLibraryId == plexLibrary.Id,
            CancellationToken
        );
        var libraryCountriesCount = await dbContext.PlexLibraryCountries.CountAsync(
            x => x.PlexLibraryId == plexLibrary.Id,
            CancellationToken
        );

        libraryActorsCount.ShouldBe(0);
        libraryGenresCount.ShouldBe(0);
        libraryCountriesCount.ShouldBe(0);
    }
}

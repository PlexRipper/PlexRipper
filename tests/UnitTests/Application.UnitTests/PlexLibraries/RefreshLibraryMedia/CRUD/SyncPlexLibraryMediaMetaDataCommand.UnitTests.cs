using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application.UnitTests;

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

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new InsertMediaMetaDataCommandResponse
            {
                PlexLibraryId = 99999, // Non-existent library ID
                PlexActors = [],
                PlexGenres = [],
                PlexCountries = [],
            }
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

        var plexLibrary = await IDbContext.PlexLibraries.FirstOrDefaultAsync();
        plexLibrary.ShouldNotBeNull();

        // Create and insert initial Plex actors
        var mediaItemActorRoles = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(50, x => x.TagKey);
        var plexActors = mediaItemActorRoles.ToPlexActor();

        var dbContext = IDbContext;
        await dbContext.PlexActors.AddRangeAsync(plexActors);
        await dbContext.SaveChangesAsync();

        var plexActorDict = plexActors.ToPlexIdDictionary(mediaItemActorRoles);

        // Create initial PlexLibraryRoles
        var initialPlexLibraryActors = plexActorDict
            .Select(x => new PlexLibraryActors(plexLibrary.Id, x.Value.Id, x.Key))
            .ToList();
        dbContext = IDbContext;
        await dbContext.PlexLibraryActors.AddRangeAsync(initialPlexLibraryActors);
        await dbContext.SaveChangesAsync();

        // Create new data with different roles
        var newPlexApiActors = FakePlexApiData.GetLibraryMediaItemActorDTO(seed).GenerateUnique(30, x => x.TagKey);
        var newPlexActors = newPlexApiActors.ToPlexActor();
        await dbContext.PlexActors.AddRangeAsync(newPlexActors);
        await dbContext.SaveChangesAsync();

        // Act
        var command = new SyncPlexLibraryMediaMetaDataCommand(
            LibraryMetadata: new InsertMediaMetaDataCommandResponse
            {
                PlexLibraryId = plexLibrary.Id,
                PlexActors = newPlexActors.ToPlexIdDictionary(newPlexApiActors),
                PlexGenres = [],
                PlexCountries = [],
            }
        );
        var result = await TestHandlerExecuteAsync(command);

        // Assert
        result.IsSuccess.ShouldBeTrue();

        dbContext = IDbContext;
        var libraryRolesList = await dbContext
            .PlexLibraryActors.Where(x => x.PlexLibraryId == plexLibrary.Id)
            .ToListAsync();

        libraryRolesList.Count.ShouldBe(newPlexApiActors.Count);
        foreach (var role in newPlexApiActors)
        {
            var roleDb = await dbContext.PlexActors.FirstOrDefaultAsync(x => x.Key == role.TagKey);
            roleDb.ShouldNotBeNull();
            libraryRolesList.ShouldContain(x => x.PlexActorId == roleDb.Id);
        }
    }
}

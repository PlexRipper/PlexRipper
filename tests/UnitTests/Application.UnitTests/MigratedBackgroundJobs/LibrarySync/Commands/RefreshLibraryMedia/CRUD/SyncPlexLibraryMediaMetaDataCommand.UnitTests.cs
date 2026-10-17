
namespace Reaparr.Application.UnitTests;

public class SyncPlexLibraryMediaMetaDataCommandUnitTests : BaseCommandUnitTest<SyncPlexLibraryMediaMetaDataCommand>
{
    [Test]
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
}

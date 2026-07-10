namespace Reaparr.Application.UnitTests;

public class GetMetadataFilterUnitTests : BaseEndpointUnitTest<GetMetadataFilter,
    GetMetadataFilterRequest, PlexMediaFilterMetadataDTO>
{
    [Test]
    public async Task ShouldReturnSuccess_WhenAllLibraryMode()
    {
        // Arrange
        await SetupDatabase(8238, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 50;
        });

        var request = new GetMetadataFilterRequest
        {
            PlexLibraryId = 0,
            MediaType = PlexMediaType.Movie,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(200);
    }

    [Test]
    public async Task ShouldReturnSuccess_WhenSingleLibraryMode()
    {
        // Arrange
        await SetupDatabase(8239, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 10;
        });

        var libraryId = await IDbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var request = new GetMetadataFilterRequest
        {
            PlexLibraryId = libraryId,
            MediaType = PlexMediaType.Movie,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(request);

        // Assert
        endpointResult.IsValid.ShouldBeTrue();
        endpointResult.StatusCode.ShouldBe(200);
    }
}

using Reaparr.BackgroundJobs.Contracts;

namespace Reaparr.Application.UnitTests;

public class RefreshLibraryMediaEndpointUnitTests : BaseUnitTest<RefreshLibraryMediaEndpoint>
{
    public RefreshLibraryMediaEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnSuccess_WhenLibrarySyncJobQueued()
    {
        // Arrange
        await SetupDatabase(
            1223,
            config =>
            {
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = IDbContext.PlexLibraries.First();

        Mock.SetupCommand(It.IsAny<QueueLibrarySyncJobCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once());

        // Act
        var rawResponse = SetupEndpointUnitTest<RefreshLibraryMediaEndpoint>();
        await rawResponse.HandleAsync(new RefreshLibraryMediaEndpointRequest(plexLibrary.Id), CancellationToken);
        var resultDTO = rawResponse.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task ShouldReturnError_WhenRefreshCommandFails()
    {
        // Arrange
        await SetupDatabase(
            1226,
            config =>
            {
                config.PlexMovieLibraryCount = 1;
            }
        );

        var plexLibrary = IDbContext.PlexLibraries.First();

        Mock.SetupCommand(It.IsAny<QueueLibrarySyncJobCommand>)
            .ReturnsAsync(Result.Fail("Failed to refresh library"))
            .Verifiable(Times.Once());

        // Act
        var rawResponse = SetupEndpointUnitTest<RefreshLibraryMediaEndpoint>();
        await rawResponse.HandleAsync(new RefreshLibraryMediaEndpointRequest(plexLibrary.Id), CancellationToken);
        var resultDTO = rawResponse.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeFalse();
        resultDTO.Errors.ShouldContain(x => x.Message.Contains("Failed to refresh library"));
    }

    [Theory]
    [InlineData(PlexMediaType.Movie)]
    [InlineData(PlexMediaType.TvShow)]
    public async Task ShouldHandleDifferentLibraryTypes_WhenTypeIsDifferent(PlexMediaType libraryType)
    {
        // Arrange
        await SetupDatabase(
            1227,
            config =>
            {
                if (libraryType == PlexMediaType.Movie)
                    config.PlexMovieLibraryCount = 1;
                else
                    config.PlexTvShowLibraryCount = 1;
            }
        );

        var plexLibrary = IDbContext.PlexLibraries.First();
        plexLibrary.Type.ShouldBe(libraryType);

        Mock.SetupCommand(It.IsAny<QueueLibrarySyncJobCommand>).ReturnsAsync(Result.Ok()).Verifiable(Times.Once());

        // Act
        var rawResponse = SetupEndpointUnitTest<RefreshLibraryMediaEndpoint>();
        await rawResponse.HandleAsync(new RefreshLibraryMediaEndpointRequest(plexLibrary.Id), CancellationToken);
        var resultDTO = rawResponse.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
    }
}

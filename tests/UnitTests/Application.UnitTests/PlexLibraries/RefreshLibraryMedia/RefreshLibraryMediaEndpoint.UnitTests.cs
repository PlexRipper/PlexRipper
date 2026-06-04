using Reaparr.BackgroundJobs.Contracts;

namespace Reaparr.Application.UnitTests;

public class RefreshLibraryMediaEndpointUnitTests
    : BaseEndpointUnitTest<RefreshLibraryMediaEndpoint, RefreshLibraryMediaEndpointRequest, BaseResultDTO>
{
    [Test]
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
        var endpointResult = await TestEndpointHandleAsync(
            new RefreshLibraryMediaEndpointRequest(plexLibrary.Id)
        );
        var resultDTO = endpointResult.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
    }

    [Test]
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
        var endpointResult = await TestEndpointHandleAsync(
            new RefreshLibraryMediaEndpointRequest(plexLibrary.Id)
        );
        var resultDTO = endpointResult.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeFalse();
        resultDTO.Errors.ShouldContain(x => x.Message.Contains("Failed to refresh library"));
    }

    [Test]
    public async Task ShouldReturnValidationFailure_WhenPlexLibraryIdIsInvalid()
    {
        // Arrange
        var request = new RefreshLibraryMediaEndpointRequest(0);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never());

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x =>
            x.PropertyName == nameof(RefreshLibraryMediaEndpointRequest.PlexLibraryId)
        );

        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Test]
    [Arguments(PlexMediaType.Movie)]
    [Arguments(PlexMediaType.TvShow)]
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
        var endpointResult = await TestEndpointHandleAsync(
            new RefreshLibraryMediaEndpointRequest(plexLibrary.Id)
        );
        var resultDTO = endpointResult.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
    }
}

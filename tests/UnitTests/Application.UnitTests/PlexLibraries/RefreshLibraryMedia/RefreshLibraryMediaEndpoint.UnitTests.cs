
namespace Reaparr.Application.UnitTests;

public class RefreshLibraryMediaEndpointUnitTests
    : BaseEndpointUnitTest<RefreshLibraryMediaEndpoint, RefreshLibraryMediaEndpointRequest, ResultDTO<PlexLibraryDTO>>
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

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(
                It.Is<QueueLibrarySyncJobCommand>(command =>
                    command.ForceLibrarySync
                    && command.ForceMediaRefresh
                    && command.PlexLibraryIds.SequenceEqual(new[] { plexLibrary.Id })
                ),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new RefreshLibraryMediaEndpointRequest
            {
                PlexLibraryId = plexLibrary.Id,
                ForceLibrarySync = true,
                ForceMediaRefresh = true,
            }
        );
        var resultDTO = endpointResult.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify();
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

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(
                It.Is<QueueLibrarySyncJobCommand>(command =>
                    command.ForceLibrarySync
                    && command.ForceMediaRefresh
                    && command.PlexLibraryIds.SequenceEqual(new[] { plexLibrary.Id })
                ),
                It.IsAny<CancellationToken>()
            ))
            .ReturnsAsync(Result.Fail("Failed to refresh library"))
            .Verifiable(Times.Once());

        // Act
        var endpointResult = await TestEndpointHandleAsync(
            new RefreshLibraryMediaEndpointRequest
            {
                PlexLibraryId = plexLibrary.Id,
                ForceLibrarySync = true,
                ForceMediaRefresh = true,
            }
        );
        var resultDTO = endpointResult.Response;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeFalse();
        resultDTO.Errors.ShouldContain(x => x.Message.Contains("Failed to refresh library"));
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnValidationFailure_WhenPlexLibraryIdIsInvalid()
    {
        // Arrange
        var request = new RefreshLibraryMediaEndpointRequest { PlexLibraryId = 0 };

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
            Times.Never()
        );
    }
}

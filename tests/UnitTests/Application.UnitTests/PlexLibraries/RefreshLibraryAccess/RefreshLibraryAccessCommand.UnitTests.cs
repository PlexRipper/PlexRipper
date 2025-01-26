using Application.Contracts;
using PlexApi.Contracts;

namespace PlexRipper.Application.UnitTests;

public class RefreshLibraryAccessCommandUnitTests : BaseUnitTest<RefreshLibraryAccessHandler>
{
    public RefreshLibraryAccessCommandUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenPlexAccountIdIsInvalid()
    {
        // Arrange
        var request = new RefreshLibraryAccessCommand(0);

        // Act
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnEmptyResult_WhenNoPlexServersAccessible()
    {
        // Arrange
        await SetupDatabase(
            1,
            config =>
            {
                config.PlexAccountCount = 1;
                config.PlexServerCount = 0;
            }
        );

        // Act
        var request = new RefreshLibraryAccessCommand(1);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldRetrieveLibrariesFromSinglePlexServer_WhenPlexAccountHasAccessToOneServer()
    {
        // Arrange
        var seed = await SetupDatabase(
            1,
            config =>
            {
                config.PlexAccountCount = 1;
                config.PlexServerCount = 1;
            }
        );
        var plexServer = IDbContext.PlexServers.FirstOrDefault();
        plexServer.ShouldNotBeNull();
        var updatedTime = DateTime.Now - TimeSpan.FromHours(9);
        var plexLibraries = FakeData.GetPlexLibrary(seed).Generate(5).ToApiLibraries(updatedTime);

        mock.Mock<IPlexApiService>()
            .Setup(x => x.GetLibrarySectionsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(plexLibraries))
            .Verifiable(Times.Once);

        mock.SetupMediator(It.IsAny<AddOrUpdatePlexLibrariesCommand>)
            .ReturnsAsync(
                Result.Ok(
                    new List<PlexLibraryAccessCrudRapport>
                    {
                        new(1, "Piet", plexServer.Name) { Created = [1, 2, 3, 4, 5] },
                    }
                )
            )
            .Verifiable(Times.Once);

        // Act
        var request = new RefreshLibraryAccessCommand(1, 1);
        var result = await _sut.Handle(request, CancellationToken.None);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeEmpty();
        result.Value.First().Created.Count.ShouldBe(5);
    }
}

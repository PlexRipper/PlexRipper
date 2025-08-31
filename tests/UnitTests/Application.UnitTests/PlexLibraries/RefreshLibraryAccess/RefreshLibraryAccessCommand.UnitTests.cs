using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.Application.UnitTests;

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
        var result = await _sut.ExecuteAsync(request, CancellationToken);

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
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Reports.ShouldBeEmpty();
        result.Value.OfflineServers.ShouldBeEmpty();
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

        mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetLibrarySectionsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(plexLibraries))
            .Verifiable(Times.Once);

        var rapport = new PlexLibraryAccessRapport("Piet", 1, plexServer.Name);

        rapport.AddGranted(1, plexLibraries.Find(x => x.Id == 1)?.Name ?? string.Empty);
        rapport.AddGranted(2, plexLibraries.Find(x => x.Id == 2)?.Name ?? string.Empty);
        rapport.AddGranted(3, plexLibraries.Find(x => x.Id == 3)?.Name ?? string.Empty);
        rapport.AddGranted(4, plexLibraries.Find(x => x.Id == 4)?.Name ?? string.Empty);
        rapport.AddGranted(5, plexLibraries.Find(x => x.Id == 5)?.Name ?? string.Empty);

        mock.SetupCommand(It.IsAny<AddOrUpdatePlexLibrariesCommand>)
            .ReturnsAsync(Result.Ok(new List<PlexLibraryAccessRapport> { rapport }))
            .Verifiable(Times.Once);

        // Act
        var request = new RefreshLibraryAccessCommand(1, 1);
        var result = await _sut.ExecuteAsync(request, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var reports = result.Value.Reports;
        reports.ShouldNotBeEmpty();
        reports.First().GetGranted.Count.ShouldBe(5);
    }
}

using NuGet.Versioning;
using Reaparr.Environment;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Reaparr.Application.UnitTests;

public class CheckForUpdatesCommandUnitTests : BaseUnitTest<CheckForUpdatesCommandHandler>
{
    [Test]
    public async Task ShouldReturnUpdateAvailable_WhenDesktopModeAndReleaseNotesFetchFails()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?>
            {
                [Environment.EnvKeys.ReaparrPlatform] = "desktop",
                [Environment.EnvKeys.InformationalVersion] = "0.36.0-dev.1",
            }
        );

        var asset = new VelopackAsset { PackageId = "Reaparr", Version = new SemanticVersion(0, 38, 0, "dev.10") };
        var updateInfo = new UpdateInfo(asset, false);
        var mockSource = new Mock<IUpdateSource>();
        var mockLocator = new Mock<IVelopackLocator>();
        var mockManager = new Mock<UpdateManager>(mockSource.Object, null!, mockLocator.Object);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetGitHubReleasesCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<IReadOnlyList<ReleaseNote>>("GitHub unavailable"))
            .Verifiable(Times.Once());
        mockManager.Setup(m => m.IsInstalled).Returns(true).Verifiable(Times.Once());
        mockManager.Setup(m => m.AppId).Returns("Reaparr").Verifiable(Times.Once());
        mockManager
            .Setup(m => m.CurrentVersion)
            .Returns(new SemanticVersion(0, 36, 0, "dev.1"))
            .Verifiable(Times.Once());
        mockManager.Setup(m => m.CheckForUpdatesAsync()).ReturnsAsync(updateInfo).Verifiable(Times.Once());
        Mock.Mock<INotificationHubService>()
            .Setup(x => x.SendRefreshNotificationAsync(RefreshDataType.UpdateAvailable, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var sut = new CheckForUpdatesCommandHandler(
            Log,
            Mock.Mock<ICommandExecutor>().Object,
            new AppBuildInfo(),
            mockManager.Object,
            Mock.Mock<INotificationHubService>().Object
        );
        var result = await sut.ExecuteAsync(new CheckForUpdatesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.IsUpdateAvailable.ShouldBeTrue();
        result.Value.NewestVersion.ShouldBe("0.38.0-dev.10");
        result.Value.ReleaseNotes.ShouldBeEmpty();
        Mock.Mock<ICommandExecutor>().Verify();
        Mock.Mock<INotificationHubService>().Verify();
        mockManager.Verify();
        mockSource.VerifyNoOtherCalls();
    }
}

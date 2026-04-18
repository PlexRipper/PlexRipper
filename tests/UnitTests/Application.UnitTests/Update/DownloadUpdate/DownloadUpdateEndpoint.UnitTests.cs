using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Reaparr.Application.UnitTests;

public class DownloadUpdateEndpointUnitTests : BaseUnitTest<DownloadUpdateEndpoint>
{
    [Test]
    public async Task ShouldReturnFailure_WhenDockerMode()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?> { [Environment.EnvKeys.ReaparrPlatform] = "docker" }
        );

        var mockSource = new Mock<IUpdateSource>();
        var mockLocator = new Mock<IVelopackLocator>();
        var mockManager = new Mock<UpdateManager>(mockSource.Object, null!, mockLocator.Object);

        // Act
        var endpoint = SetupEndpointUnitTest<DownloadUpdateEndpoint>(s => s.AddSingleton(_ => mockManager.Object));
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem();
        result.Errors[0].Message.ShouldBe("Desktop updates are not supported in the current runtime mode");
    }

    [Test]
    public async Task ShouldReturnSuccess_WhenDesktopModeAndUpdateAvailable()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?> { [Environment.EnvKeys.ReaparrPlatform] = "desktop" }
        );

        var asset = new VelopackAsset { PackageId = "Reaparr", Version = new SemanticVersion(9, 9, 9) };
        var updateInfo = new UpdateInfo(asset, false, null!, null!);

        var mockSource = new Mock<IUpdateSource>();
        var mockLocator = new Mock<IVelopackLocator>();
        var mockManager = new Mock<UpdateManager>(mockSource.Object, null!, mockLocator.Object);
        mockManager.Setup(m => m.CheckForUpdatesAsync()).ReturnsAsync(updateInfo).Verifiable(Times.Once());
        mockManager
            .Setup(m => m.DownloadUpdatesAsync(It.IsAny<UpdateInfo>(), null, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<DownloadUpdateEndpoint>(s => s.AddSingleton(_ => mockManager.Object));
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        mockManager.Verify();
    }
}

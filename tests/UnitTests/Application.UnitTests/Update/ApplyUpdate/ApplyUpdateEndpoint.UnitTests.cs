using Microsoft.Extensions.DependencyInjection;
using NuGet.Versioning;
using Velopack;
using Velopack.Locators;
using Velopack.Sources;

namespace Reaparr.Application.UnitTests;

public class ApplyUpdateEndpointUnitTests : BaseUnitTest<ApplyUpdateEndpoint>
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
        var endpoint = SetupEndpointUnitTest<ApplyUpdateEndpoint>(s => s.AddSingleton(_ => mockManager.Object));
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldHaveSingleItem();
        result.Errors[0].Message.ShouldBe("Desktop updates are not supported in the current runtime mode");
        mockManager.Verify(m => m.UpdatePendingRestart, Times.Never);
        mockSource.VerifyNoOtherCalls();
    }

    [Test]
    public async Task ShouldReturnSuccess_WhenDesktopModeAndUpdatePendingRestart()
    {
        // Arrange
        using var _ = WithEnvironmentVariablesAsync(
            new Dictionary<string, string?> { [Environment.EnvKeys.ReaparrPlatform] = "desktop" }
        );

        var asset = new VelopackAsset { PackageId = "Reaparr", Version = new SemanticVersion(9, 9, 9) };

        var mockSource = new Mock<IUpdateSource>();
        var mockLocator = new Mock<IVelopackLocator>();
        var mockManager = new Mock<UpdateManager>(mockSource.Object, null!, mockLocator.Object);
        mockManager.Setup(m => m.UpdatePendingRestart).Returns(asset).Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<ApplyUpdateEndpoint>(s => s.AddSingleton(_ => mockManager.Object));
        await endpoint.HandleAsync(CancellationToken);
        var result = endpoint.Response;

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();

        mockManager.Verify(m => m.UpdatePendingRestart, Times.Once);
        mockSource.VerifyNoOtherCalls();
    }
}

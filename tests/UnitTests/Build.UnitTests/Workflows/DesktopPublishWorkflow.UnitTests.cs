using System.IO.Abstractions.TestingHelpers;

namespace Reaparr.Build.UnitTests;

internal class DesktopPublishBuildCommandHandlerUnitTests : BaseUnitTest<DesktopPublishBuildCommandHandler>
{
    [Test]
    public async Task ShouldFail_WhenVersionIsMissing()
    {
        // Arrange
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            InformationalVersion = "1.2.3-dev.1",
            DryRun = true,
        };

        // Act
        var result = await Sut.ExecuteAsync(new DesktopPublishBuildCommand(settings), CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReuseExistingFrontendOutput_WhenFrontendPublicDirectoryAlreadyExists()
    {
        // Arrange
        SetupFileSystem(system =>
        {
            system.AddDirectory("/repo/src/AppHost/ClientApp/.output/public");
            system.AddFile("/repo/src/AppHost/ClientApp/.output/public/index.html", new MockFileData("frontend"));
        });

        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            DryRun = true,
            SkipRestore = true,
        };

        // Act
        var result = await Sut.ExecuteAsync(new DesktopPublishBuildCommand(settings), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(0);
    }
}

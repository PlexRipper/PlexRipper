namespace Reaparr.Build.UnitTests;

public class DesktopCiPackageCommandUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldPreserveLaunchMode_WhenCreatingCiSettings()
    {
        // Arrange
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "1.2.3",
            InformationalVersion = "1.2.3-dev.1",
            Channel = "linux-x64-dev",
            SkipFrontend = false,
            SkipRestore = true,
            SkipPackage = true,
            DryRun = true,
            FrontendPublicDirectory = ".tmp/frontend-public",
            ArtifactDirectory = "Releases",
            PreserveExistingArtifacts = true,
            LaunchMode = "packaged",
        };

        // Act
        var ciSettings = DesktopCiPackageCommand.CreateCiSettings(settings);

        // Assert
        ciSettings.LaunchMode.ShouldBe("packaged");
        ciSettings.SkipFrontend.ShouldBeTrue();
        ciSettings.SkipRestore.ShouldBeFalse();
        ciSettings.SkipPackage.ShouldBeFalse();
    }
}

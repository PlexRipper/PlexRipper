using System.Reflection;
using Reaparr.Build;

namespace Reaparr.AppHost.UnitTests;

public class DesktopCiPackageCommandUnitTests
{
    [Test]
    public void ShouldNotSkipRestore_WhenCreatingCiSettings()
    {
        // Arrange
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "9.9.9",
            InformationalVersion = "9.9.9-dev",
            FrontendPublicDirectory = Path.Combine(".tmp", "frontend-public"),
            ArtifactDirectory = "Releases",
            PreserveExistingArtifacts = true,
            DryRun = true,
        };

        // Act
        var ciSettings = GetCiSettings(settings);

        // Assert
        ciSettings.RuntimeIdentifier.ShouldBe(settings.RuntimeIdentifier);
        ciSettings.Version.ShouldBe(settings.Version);
        ciSettings.InformationalVersion.ShouldBe(settings.InformationalVersion);
        ciSettings.Channel.ShouldBe(settings.Channel);
        ciSettings.FrontendPublicDirectory.ShouldBe(settings.FrontendPublicDirectory);
        ciSettings.ArtifactDirectory.ShouldBe(settings.ArtifactDirectory);
        ciSettings.PreserveExistingArtifacts.ShouldBeTrue();
        ciSettings.DryRun.ShouldBeTrue();
        ciSettings.SkipFrontend.ShouldBeTrue();
        ciSettings.SkipRestore.ShouldBeFalse();
        ciSettings.SkipPackage.ShouldBeFalse();
    }

    private static DesktopCommandSettings GetCiSettings(DesktopCommandSettings settings)
    {
        var methodInfo = typeof(DesktopCiPackageCommand).GetMethod(
            "CreateCiSettings",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        methodInfo.ShouldNotBeNull();

        var result = methodInfo.Invoke(null, [settings]);
        result.ShouldBeOfType<DesktopCommandSettings>();

        return (DesktopCommandSettings)result;
    }
}

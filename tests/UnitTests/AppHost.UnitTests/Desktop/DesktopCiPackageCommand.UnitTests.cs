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

    [Test]
    public void ShouldIncludeRuntimeArgument_WhenCreatingVelopackPackArguments()
    {
        // Arrange
        var runtime = DesktopRuntimeCatalog.Get("linux-x64");
        var settings = new DesktopCommandSettings
        {
            RuntimeIdentifier = "linux-x64",
            Version = "9.9.9",
            InformationalVersion = "9.9.9-dev",
            Channel = "dev",
            ArtifactDirectory = "Releases",
            PreserveExistingArtifacts = true,
            DryRun = true,
        };

        var paths = new BuildPaths(new DirectoryInfo("/tmp/reaparr"));

        // Act
        var arguments = GetPackArguments(paths, runtime, settings);

        // Assert
        arguments.ShouldContain("--runtime");
        var runtimeIndex = arguments.IndexOf("--runtime");
        runtimeIndex.ShouldBeGreaterThanOrEqualTo(0);
        arguments[runtimeIndex + 1].ShouldBe("linux-x64");

        arguments.ShouldContain("--mainExe");
        var mainExeIndex = arguments.IndexOf("--mainExe");
        mainExeIndex.ShouldBeGreaterThanOrEqualTo(0);
        arguments[mainExeIndex + 1].ShouldBe("Reaparr.AppHost");
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

    private static List<string> GetPackArguments(
        BuildPaths paths,
        DesktopRuntime runtime,
        DesktopCommandSettings settings
    )
    {
        var methodInfo = typeof(DesktopPackageWorkflow).GetMethod(
            "CreatePackArguments",
            BindingFlags.NonPublic | BindingFlags.Static
        );

        methodInfo.ShouldNotBeNull();

        var result = methodInfo.Invoke(null, [paths, runtime, settings, null]);
        result.ShouldBeOfType<List<string>>();

        return (List<string>)result;
    }
}

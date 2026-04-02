using Reaparr.Environment;

namespace Reaparr.BaseTests.UnitTests;

[NotInParallel]
public sealed class PathProviderUnitTests : BaseUnitTest
{
    private readonly Dictionary<string, string?> _originalEnvironmentVariables = new()
    {
        ["REAPARR_PLATFORM"] = System.Environment.GetEnvironmentVariable("REAPARR_PLATFORM"),
        ["REAPARR_DATA_PATH"] = System.Environment.GetEnvironmentVariable("REAPARR_DATA_PATH"),
        ["REAPARR_CONFIG_PATH"] = System.Environment.GetEnvironmentVariable("REAPARR_CONFIG_PATH"),
        ["HOME"] = System.Environment.GetEnvironmentVariable("HOME"),
        ["APPDATA"] = System.Environment.GetEnvironmentVariable("APPDATA"),
    };

    [Test]
    public void DefaultDownloadsDestinationFolder_ShouldUseUserDownloads_WhenDesktopMode()
    {
        // Arrange
        SetEnvironmentVariable("REAPARR_PLATFORM", "desktop");
        SetEnvironmentVariable("REAPARR_DATA_PATH", null);
        SetEnvironmentVariable("REAPARR_CONFIG_PATH", null);

        var expected = Path.Combine(GetHomeDirectory(), "Downloads");

        // Act
        var path = PathProvider.DefaultDownloadsDestinationFolder;

        // Assert
        path.ShouldBe(expected);
    }

    [Test]
    public void ConfigDirectory_ShouldUseSeparateDesktopConfigRoot_WhenDesktopMode()
    {
        // Arrange
        SetEnvironmentVariable("REAPARR_PLATFORM", "desktop");
        SetEnvironmentVariable("REAPARR_DATA_PATH", null);
        SetEnvironmentVariable("REAPARR_CONFIG_PATH", null);

        var expected = OsInfo.CurrentOS switch
        {
            OperatingSystemPlatform.Windows => Path.Combine(GetAppDataDirectory(), "Reaparr"),
            OperatingSystemPlatform.Osx => Path.Combine(
                GetHomeDirectory(),
                "Library",
                "Application Support",
                "Reaparr"
            ),
            _ => Path.Combine(GetHomeDirectory(), ".config", "Reaparr"),
        };

        // Act
        var configDirectory = PathProvider.ConfigDirectory;

        // Assert
        configDirectory.ShouldBe(expected);
        PathProvider.DatabasePath.ShouldBe(Path.Combine(expected, PathProvider.DatabaseName));
    }

    [Test]
    public void Paths_ShouldRespectExplicitConfigAndDataOverrides()
    {
        // Arrange
        var mediaPath = Path.Combine(Path.GetTempPath(), "reaparr-media");
        var configPath = Path.Combine(Path.GetTempPath(), "reaparr-config");

        SetEnvironmentVariable("REAPARR_PLATFORM", "desktop");
        SetEnvironmentVariable("REAPARR_DATA_PATH", mediaPath);
        SetEnvironmentVariable("REAPARR_CONFIG_PATH", configPath);

        // Act / Assert
        PathProvider.DataDirectory.ShouldBe(mediaPath);
        PathProvider.DefaultMovieDestinationFolder.ShouldBe(Path.Combine(mediaPath, "Movies"));
        PathProvider.ConfigDirectory.ShouldBe(configPath);
        PathProvider.DatabasePath.ShouldBe(Path.Combine(configPath, "ReaparrDB.db"));
    }

    [Test]
    public void DefaultDownloadsDestinationFolder_ShouldFallbackToHomeReaparr_WhenDownloadsPathIsNotWritable()
    {
        // Arrange
        var sandboxRoot = Path.Combine(Path.GetTempPath(), $"reaparr-path-provider-{Guid.NewGuid():N}");
        Directory.CreateDirectory(sandboxRoot);
        File.WriteAllText(Path.Combine(sandboxRoot, "Downloads"), "blocked");

        SetEnvironmentVariable("REAPARR_PLATFORM", "desktop");
        SetEnvironmentVariable("REAPARR_DATA_PATH", null);
        SetEnvironmentVariable("REAPARR_CONFIG_PATH", null);
        SetEnvironmentVariable("HOME", sandboxRoot);

        var expected = Path.Combine(sandboxRoot, "Reaparr", "Downloads");

        // Act
        var path = PathProvider.DefaultDownloadsDestinationFolder;

        // Assert
        path.ShouldBe(expected);
    }

    public override void Dispose()
    {
        foreach (var (key, value) in _originalEnvironmentVariables)
        {
            SetEnvironmentVariable(key, value);
        }

        base.Dispose();
    }

    private static void SetEnvironmentVariable(string key, string? value) =>
        System.Environment.SetEnvironmentVariable(key, value);

    private static string GetAppDataDirectory() =>
        System.Environment.GetEnvironmentVariable("APPDATA")
        ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

    private static string GetHomeDirectory() =>
        System.Environment.GetEnvironmentVariable("HOME")
        ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
}

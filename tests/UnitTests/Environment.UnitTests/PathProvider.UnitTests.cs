using Reaparr.Environment;

namespace Reaparr.Environment.UnitTests;

[NotInParallel]
public class PathProviderUnitTests
{
    private static readonly string ReaparrPlatformKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrPlatform
    );

    private static readonly string ReaparrDataPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrDataPath
    );

    private static readonly string ReaparrConfigPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrConfigPath
    );

    private static readonly string ReaparrDownloadsPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrDownloadsPath
    );

    private static readonly string ReaparrMoviesPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrMoviesPath
    );

    private static readonly string ReaparrTvShowsPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrTvShowsPath
    );

    private static readonly string ReaparrMusicPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrMusicPath
    );

    private static readonly string ReaparrPhotosPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrPhotosPath
    );

    private static readonly string ReaparrOtherPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrOtherPath
    );

    private static readonly string ReaparrGamesPathKey = EnvironmentExtensions.EnvironmentVariableName(
        EnvironmentVariableKey.ReaparrGamesPath
    );
    private const string HomeKey = "HOME";
    private const string AppDataKey = "APPDATA";

    [Test]
    public void ShouldExposeExpectedDefaultNames_WhenReadingStaticNameProperties()
    {
        // Assert
        PathProvider.DefaultConfigFolderName.ShouldBe("Config");
        PathProvider.DefaultMovieFolderName.ShouldBe("Movies");
        PathProvider.DefaultDownloadsFolderName.ShouldBe("Downloads");
        PathProvider.DefaultTvShowsFolderName.ShouldBe("TvShows");
        PathProvider.DefaultMusicFolderName.ShouldBe("Music");
        PathProvider.DefaultPhotosFolderName.ShouldBe("Photos");
        PathProvider.DefaultOtherFolderName.ShouldBe("Other");
        PathProvider.DefaultGamesFolderName.ShouldBe("Games");

        PathProvider.ConfigFileName.ShouldBe("ReaparrSettings.json");
        PathProvider.DatabaseName.ShouldBe("ReaparrDB.db");
        PathProvider.DatabaseShmName.ShouldBe("ReaparrDB.db-shm");
        PathProvider.DatabaseWalName.ShouldBe("ReaparrDB.db-wal");
    }

    [Test]
    public void ShouldUseConfiguredConfigPath_WhenConfigPathIsSet()
    {
        // Arrange
        var configuredConfigPath = "/test/config";

        WithEnvironment(
            "desktop",
            null,
            configuredConfigPath,
            "/test/home",
            "/test/appdata",
            () =>
            {
                // Act & Assert
                PathProvider.ConfigDirectory.ShouldBe(configuredConfigPath);
                PathProvider.ConfigFileLocation.ShouldBe(Path.Join(configuredConfigPath, PathProvider.ConfigFileName));
                PathProvider.DatabaseBackupDirectory.ShouldBe(Path.Combine(configuredConfigPath, "Database BackUp"));
                PathProvider.DatabasePath.ShouldBe(Path.Combine(configuredConfigPath, PathProvider.DatabaseName));
                PathProvider.Database_SHM_Path.ShouldBe(
                    Path.Combine(configuredConfigPath, PathProvider.DatabaseShmName)
                );
                PathProvider.Database_WAL_Path.ShouldBe(
                    Path.Combine(configuredConfigPath, PathProvider.DatabaseWalName)
                );
                PathProvider.LogsDirectory.ShouldBe(Path.Combine(configuredConfigPath, "Logs"));
            }
        );
    }

    [Test]
    public void ShouldTrimConfiguredConfigPath_WhenConfigPathContainsPadding()
    {
        // Arrange
        const string configuredConfigPath = "  /test/config-padded  ";

        WithEnvironment(
            "desktop",
            null,
            configuredConfigPath,
            "/test/home",
            "/test/appdata",
            () =>
            {
                // Assert
                PathProvider.ConfigDirectory.ShouldBe("/test/config-padded");
            }
        );
    }

    [Test]
    public void ShouldFallbackToDesktopConfigPath_WhenConfigPathIsUnsetAndDesktopMode()
    {
        // Arrange
        var home = "/home/test-user";
        var appData = "/appdata/test-user";

        WithEnvironment(
            "desktop",
            null,
            null,
            home,
            appData,
            () =>
            {
                // Act
                var expected = GetExpectedDesktopConfigPath(home, appData);

                // Assert
                PathProvider.ConfigDirectory.ShouldBe(expected);
            }
        );
    }

    [Test]
    public void ShouldFallbackToDockerConfigPath_WhenConfigPathIsUnsetAndDockerMode()
    {
        // Arrange
        WithEnvironment(
            "docker",
            null,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Act
                var expected = Path.Combine(GetExpectedDockerRootDirectory(), "config");

                // Assert
                PathProvider.ConfigDirectory.ShouldBe(expected);
            }
        );
    }

    [Test]
    public void ShouldTreatWhitespaceConfigPathAsUnset_WhenDesktopMode()
    {
        // Arrange
        var home = "/home/whitespace";
        var appData = "/appdata/whitespace";

        WithEnvironment(
            "desktop",
            null,
            "   ",
            home,
            appData,
            () =>
            {
                // Act
                var expected = GetExpectedDesktopConfigPath(home, appData);

                // Assert
                PathProvider.ConfigDirectory.ShouldBe(expected);
            }
        );
    }

    [Test]
    public void ShouldUseConfiguredDataPath_WhenDataPathIsSet()
    {
        // Arrange
        const string configuredDataPath = "/test/data";

        WithEnvironment(
            "docker",
            configuredDataPath,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe(configuredDataPath);
            }
        );
    }

    [Test]
    public void ShouldTrimConfiguredDataPath_WhenDataPathContainsPadding()
    {
        // Arrange
        const string configuredDataPath = "  /test/data-padded  ";

        WithEnvironment(
            "docker",
            configuredDataPath,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe("/test/data-padded");
            }
        );
    }

    [Test]
    public void ShouldFallbackToHomeDirectory_WhenDataPathIsUnsetAndDesktopMode()
    {
        // Arrange
        const string home = "/home/data-fallback";

        WithEnvironment(
            "desktop",
            null,
            null,
            home,
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe(home);
            }
        );
    }

    [Test]
    public void ShouldFallbackToDockerRoot_WhenDataPathIsUnsetAndDockerMode()
    {
        // Arrange
        WithEnvironment(
            "docker",
            null,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe(GetExpectedDockerRootDirectory());
            }
        );
    }

    [Test]
    public void ShouldTreatWhitespaceDataPathAsUnset_WhenDockerMode()
    {
        // Arrange
        WithEnvironment(
            "docker",
            "  \t  ",
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe(GetExpectedDockerRootDirectory());
            }
        );
    }

    [Test]
    public void ShouldDefaultToDockerMode_WhenPlatformIsUnknown()
    {
        // Arrange
        WithEnvironment(
            "unknown",
            null,
            null,
            "/home/unknown-mode",
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe(GetExpectedDockerRootDirectory());
                PathProvider.ConfigDirectory.ShouldBe(Path.Combine(GetExpectedDockerRootDirectory(), "config"));
            }
        );
    }

    [Test]
    public void ShouldTreatWhitespaceAroundDesktopPlatformAsDesktop_WhenValueIsTrimmed()
    {
        // Arrange
        const string home = "/home/whitespace-mode";

        WithEnvironment(
            " desktop ",
            null,
            null,
            home,
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DataDirectory.ShouldBe(home);
            }
        );
    }

    [Test]
    [Arguments("Movies")]
    [Arguments("TvShows")]
    [Arguments("Music")]
    [Arguments("Photos")]
    [Arguments("Other")]
    [Arguments("Games")]
    public void ShouldBuildDefaultMediaDestinationFoldersFromDataPath_WhenDataPathIsConfigured(string folderName)
    {
        // Arrange
        const string dataPath = "/test/media-data";

        WithEnvironment(
            "docker",
            dataPath,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Act
                var expected = Path.Combine(dataPath, folderName);

                // Assert
                GetDefaultDestinationFolder(folderName).ShouldBe(expected);
            }
        );
    }

    [Test]
    [Arguments("Movies")]
    [Arguments("Downloads")]
    [Arguments("TvShows")]
    [Arguments("Music")]
    [Arguments("Photos")]
    [Arguments("Other")]
    [Arguments("Games")]
    public void ShouldBuildDefaultMediaDestinationFoldersFromHome_WhenDesktopModeAndDataPathUnset(string folderName)
    {
        // Arrange
        const string home = "/test/media-home";

        WithEnvironment(
            "desktop",
            null,
            null,
            home,
            "/unused/appdata",
            () =>
            {
                // Act
                var expected = folderName switch
                {
                    "Downloads" => Path.Combine(home, "Downloads", "Reaparr"),
                    "Movies" => Path.Combine(home, "Videos", PathProvider.DefaultMovieFolderName),
                    "TvShows" => Path.Combine(home, "Videos", PathProvider.DefaultTvShowsFolderName),
                    "Music" => Path.Combine(home, "Music"),
                    "Photos" => Path.Combine(home, "Pictures"),
                    "Other" => Path.Combine(home, PathProvider.DefaultOtherFolderName),
                    "Games" => Path.Combine(home, PathProvider.DefaultGamesFolderName),
                    _ => throw new InvalidOperationException($"Unsupported folder: {folderName}"),
                };

                // Assert
                GetDefaultDestinationFolder(folderName).ShouldBe(expected);
            }
        );
    }

    [Test]
    [Arguments("Movies")]
    [Arguments("Downloads")]
    [Arguments("TvShows")]
    [Arguments("Music")]
    [Arguments("Photos")]
    [Arguments("Other")]
    [Arguments("Games")]
    public void ShouldBuildDefaultMediaDestinationFoldersFromDockerRoot_WhenDockerModeAndDataPathUnset(
        string folderName
    )
    {
        // Arrange
        WithEnvironment(
            "docker",
            null,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Act
                var expected =
                    folderName == "Downloads"
                        ? Path.Combine(GetExpectedDockerRootDirectory(), "downloads")
                        : Path.Combine(GetExpectedDockerRootDirectory(), "media", folderName);

                // Assert
                GetDefaultDestinationFolder(folderName).ShouldBe(expected);
            }
        );
    }

    [Test]
    public void ShouldExposeExpectedDatabaseFilesInOrder_WhenReadingDatabaseFilesProperty()
    {
        // Arrange
        const string configPath = "/test/db-config";

        WithEnvironment(
            "desktop",
            null,
            configPath,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Act
                var files = new PathProvider().DatabaseFiles;

                // Assert
                files.Count.ShouldBe(3);
                files[0].ShouldBe(PathProvider.DatabasePath);
                files[1].ShouldBe(PathProvider.Database_SHM_Path);
                files[2].ShouldBe(PathProvider.Database_WAL_Path);
            }
        );
    }

    [Test]
    public void ShouldMapIPathProviderMembersToStaticPathProviderValues()
    {
        // Arrange
        const string dataPath = "/test/interface-data";
        const string configPath = "/test/interface-config";

        WithEnvironment(
            "desktop",
            dataPath,
            configPath,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Act
                IPathProvider sut = new PathProvider();

                // Assert
                sut.RootDirectory.ShouldBe(PathProvider.DataDirectory);
                sut.ConfigDirectory.ShouldBe(PathProvider.ConfigDirectory);
                sut.ConfigFileLocation.ShouldBe(PathProvider.ConfigFileLocation);
                sut.ConfigFileName.ShouldBe(PathProvider.ConfigFileName);
                sut.DatabaseBackupDirectory.ShouldBe(PathProvider.DatabaseBackupDirectory);
                sut.DatabaseName.ShouldBe(PathProvider.DatabaseName);
                sut.DatabasePath.ShouldBe(PathProvider.DatabasePath);
                sut.LogsDirectory.ShouldBe(PathProvider.LogsDirectory);
                sut.DatabaseFiles.ShouldBe([
                    PathProvider.DatabasePath,
                    PathProvider.Database_SHM_Path,
                    PathProvider.Database_WAL_Path,
                ]);
            }
        );
    }

    [Test]
    public void ShouldUseConfiguredDownloadsPath_WhenDownloadsPathIsSet()
    {
        // Arrange
        const string configuredDownloadsPath = "/test/custom-downloads";

        WithEnvironment(
            "desktop",
            "/test/data-path-ignored",
            null,
            "/test/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                PathProvider.DefaultDownloadsDestinationFolder.ShouldBe(configuredDownloadsPath);
            },
            downloadsPath: configuredDownloadsPath
        );
    }

    [Test]
    [Arguments("Movies")]
    [Arguments("TvShows")]
    [Arguments("Music")]
    [Arguments("Photos")]
    [Arguments("Other")]
    [Arguments("Games")]
    public void ShouldUsePerTypePathOverride_WhenPerTypeEnvVarIsSet(string folderName)
    {
        // Arrange
        const string perTypePath = "/test/per-type-path";

        WithEnvironment(
            "desktop",
            "/test/data-fallback",
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                GetDefaultDestinationFolder(folderName).ShouldBe(perTypePath);
            },
            moviesPath: folderName == "Movies" ? perTypePath : null,
            tvShowsPath: folderName == "TvShows" ? perTypePath : null,
            musicPath: folderName == "Music" ? perTypePath : null,
            photosPath: folderName == "Photos" ? perTypePath : null,
            otherPath: folderName == "Other" ? perTypePath : null,
            gamesPath: folderName == "Games" ? perTypePath : null
        );
    }

    [Test]
    [Arguments("Movies")]
    [Arguments("TvShows")]
    [Arguments("Music")]
    [Arguments("Photos")]
    [Arguments("Other")]
    [Arguments("Games")]
    public void ShouldFallbackToDataPathForLibraries_WhenPerTypeEnvVarIsUnset(string folderName)
    {
        // Arrange
        const string dataPath = "/test/data-fallback";

        WithEnvironment(
            "desktop",
            dataPath,
            null,
            "/unused/home",
            "/unused/appdata",
            () =>
            {
                // Assert
                GetDefaultDestinationFolder(folderName).ShouldBe(Path.Combine(dataPath, folderName));
            }
        );
    }

    private static string GetExpectedDesktopConfigPath(string home, string appData) =>
        OsInfo.CurrentOS switch
        {
            OperatingSystemPlatform.Windows => Path.Combine(appData, "Reaparr"),
            OperatingSystemPlatform.Osx => Path.Combine(home, "Library", "Application Support", "Reaparr"),
            _ => Path.Combine(home, ".config", "Reaparr"),
        };

    private static string GetExpectedDockerRootDirectory() =>
        OsInfo.CurrentOS switch
        {
            OperatingSystemPlatform.Windows => Path.GetPathRoot(typeof(PathProvider).Assembly.Location) ?? @"C:\\",
            _ => "/",
        };

    private static string GetDefaultDestinationFolder(string folderName) =>
        folderName switch
        {
            "Movies" => PathProvider.DefaultMovieDestinationFolder,
            "Downloads" => PathProvider.DefaultDownloadsDestinationFolder,
            "TvShows" => PathProvider.DefaultTvShowsDestinationFolder,
            "Music" => PathProvider.DefaultMusicDestinationFolder,
            "Photos" => PathProvider.DefaultPhotosDestinationFolder,
            "Other" => PathProvider.DefaultOtherDestinationFolder,
            "Games" => PathProvider.DefaultGamesDestinationFolder,
            _ => throw new InvalidOperationException($"Unsupported folder: {folderName}"),
        };

    private static void WithEnvironment(
        string? platform,
        string? dataPath,
        string? configPath,
        string? home,
        string? appData,
        Action assertion,
        string? downloadsPath = null,
        string? moviesPath = null,
        string? tvShowsPath = null,
        string? musicPath = null,
        string? photosPath = null,
        string? otherPath = null,
        string? gamesPath = null
    )
    {
        var originalValues = new Dictionary<string, string?>
        {
            [ReaparrPlatformKey] = System.Environment.GetEnvironmentVariable(ReaparrPlatformKey),
            [ReaparrDataPathKey] = System.Environment.GetEnvironmentVariable(ReaparrDataPathKey),
            [ReaparrConfigPathKey] = System.Environment.GetEnvironmentVariable(ReaparrConfigPathKey),
            [ReaparrDownloadsPathKey] = System.Environment.GetEnvironmentVariable(ReaparrDownloadsPathKey),
            [ReaparrMoviesPathKey] = System.Environment.GetEnvironmentVariable(ReaparrMoviesPathKey),
            [ReaparrTvShowsPathKey] = System.Environment.GetEnvironmentVariable(ReaparrTvShowsPathKey),
            [ReaparrMusicPathKey] = System.Environment.GetEnvironmentVariable(ReaparrMusicPathKey),
            [ReaparrPhotosPathKey] = System.Environment.GetEnvironmentVariable(ReaparrPhotosPathKey),
            [ReaparrOtherPathKey] = System.Environment.GetEnvironmentVariable(ReaparrOtherPathKey),
            [ReaparrGamesPathKey] = System.Environment.GetEnvironmentVariable(ReaparrGamesPathKey),
            [HomeKey] = System.Environment.GetEnvironmentVariable(HomeKey),
            [AppDataKey] = System.Environment.GetEnvironmentVariable(AppDataKey),
        };

        try
        {
            System.Environment.SetEnvironmentVariable(ReaparrPlatformKey, platform);
            System.Environment.SetEnvironmentVariable(ReaparrDataPathKey, dataPath);
            System.Environment.SetEnvironmentVariable(ReaparrConfigPathKey, configPath);
            System.Environment.SetEnvironmentVariable(ReaparrDownloadsPathKey, downloadsPath);
            System.Environment.SetEnvironmentVariable(ReaparrMoviesPathKey, moviesPath);
            System.Environment.SetEnvironmentVariable(ReaparrTvShowsPathKey, tvShowsPath);
            System.Environment.SetEnvironmentVariable(ReaparrMusicPathKey, musicPath);
            System.Environment.SetEnvironmentVariable(ReaparrPhotosPathKey, photosPath);
            System.Environment.SetEnvironmentVariable(ReaparrOtherPathKey, otherPath);
            System.Environment.SetEnvironmentVariable(ReaparrGamesPathKey, gamesPath);
            System.Environment.SetEnvironmentVariable(HomeKey, home);
            System.Environment.SetEnvironmentVariable(AppDataKey, appData);

            assertion();
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(ReaparrPlatformKey, originalValues[ReaparrPlatformKey]);
            System.Environment.SetEnvironmentVariable(ReaparrDataPathKey, originalValues[ReaparrDataPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrConfigPathKey, originalValues[ReaparrConfigPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrDownloadsPathKey, originalValues[ReaparrDownloadsPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrMoviesPathKey, originalValues[ReaparrMoviesPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrTvShowsPathKey, originalValues[ReaparrTvShowsPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrMusicPathKey, originalValues[ReaparrMusicPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrPhotosPathKey, originalValues[ReaparrPhotosPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrOtherPathKey, originalValues[ReaparrOtherPathKey]);
            System.Environment.SetEnvironmentVariable(ReaparrGamesPathKey, originalValues[ReaparrGamesPathKey]);
            System.Environment.SetEnvironmentVariable(HomeKey, originalValues[HomeKey]);
            System.Environment.SetEnvironmentVariable(AppDataKey, originalValues[AppDataKey]);
        }
    }
}

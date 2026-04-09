namespace Reaparr.Environment.UnitTests;

[NotInParallel]
public class PathProviderUnitTests
{
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
                var expected = Path.Combine(GetExpectedDockerRootDirectory(), PathProvider.DefaultConfigFolderName);

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
                PathProvider.DataDirectory.ShouldBe(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)
                );
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
                PathProvider.ConfigDirectory.ShouldBe(
                    Path.Combine(GetExpectedDockerRootDirectory(), PathProvider.DefaultConfigFolderName)
                );
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
                PathProvider.DataDirectory.ShouldBe(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile)
                );
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
                var desktopMediaRoot = Path.Combine(
                    System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyVideos),
                    PathProvider.DefaultReaparrFolderName
                );
                var expected = folderName switch
                {
                    "Downloads" => Path.Combine(
                        System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                        PathProvider.DefaultDownloadsFolderName,
                        PathProvider.DefaultReaparrFolderName
                    ),
                    "Movies" => Path.Combine(desktopMediaRoot, PathProvider.DefaultMovieFolderName),
                    "TvShows" => Path.Combine(desktopMediaRoot, PathProvider.DefaultTvShowsFolderName),
                    "Music" => Path.Combine(desktopMediaRoot, PathProvider.DefaultMusicFolderName),
                    "Photos" => Path.Combine(desktopMediaRoot, PathProvider.DefaultPhotosFolderName),
                    "Other" => Path.Combine(desktopMediaRoot, PathProvider.DefaultOtherFolderName),
                    "Games" => Path.Combine(desktopMediaRoot, PathProvider.DefaultGamesFolderName),
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
                var expected = Path.Combine(
                    "/",
                    folderName == "Downloads" ? PathProvider.DefaultDownloadsFolderName : folderName
                );

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

    private static string GetExpectedDesktopConfigPath(string _, string __) =>
        OsInfo.CurrentOS switch
        {
            OperatingSystemPlatform.Windows => Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
                PathProvider.DefaultReaparrFolderName
            ),
            OperatingSystemPlatform.Osx => Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Library",
                "Application Support",
                PathProvider.DefaultReaparrFolderName
            ),
            _ => Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                ".config",
                PathProvider.DefaultReaparrFolderName
            ),
        };

    private static string GetExpectedDockerRootDirectory() => "/";

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
            [EnvKeys.ReaparrPlatform] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrPlatform),
            [EnvKeys.ReaparrDataPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrDataPath),
            [EnvKeys.ReaparrConfigPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrConfigPath),
            [EnvKeys.ReaparrDownloadsPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrDownloadsPath),
            [EnvKeys.ReaparrMoviesPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrMoviesPath),
            [EnvKeys.ReaparrTvShowsPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrTvShowsPath),
            [EnvKeys.ReaparrMusicPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrMusicPath),
            [EnvKeys.ReaparrPhotosPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrPhotosPath),
            [EnvKeys.ReaparrOtherPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrOtherPath),
            [EnvKeys.ReaparrGamesPath] = System.Environment.GetEnvironmentVariable(EnvKeys.ReaparrGamesPath),
            [EnvKeys.Home] = System.Environment.GetEnvironmentVariable(EnvKeys.Home),
            [EnvKeys.AppData] = System.Environment.GetEnvironmentVariable(EnvKeys.AppData),
        };

        try
        {
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrPlatform, platform);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrDataPath, dataPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrConfigPath, configPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrDownloadsPath, downloadsPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrMoviesPath, moviesPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrTvShowsPath, tvShowsPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrMusicPath, musicPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrPhotosPath, photosPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrOtherPath, otherPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrGamesPath, gamesPath);
            System.Environment.SetEnvironmentVariable(EnvKeys.Home, home);
            System.Environment.SetEnvironmentVariable(EnvKeys.AppData, appData);

            assertion();
        }
        finally
        {
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrPlatform, originalValues[EnvKeys.ReaparrPlatform]);
            System.Environment.SetEnvironmentVariable(EnvKeys.ReaparrDataPath, originalValues[EnvKeys.ReaparrDataPath]);
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrConfigPath,
                originalValues[EnvKeys.ReaparrConfigPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrDownloadsPath,
                originalValues[EnvKeys.ReaparrDownloadsPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrMoviesPath,
                originalValues[EnvKeys.ReaparrMoviesPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrTvShowsPath,
                originalValues[EnvKeys.ReaparrTvShowsPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrMusicPath,
                originalValues[EnvKeys.ReaparrMusicPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrPhotosPath,
                originalValues[EnvKeys.ReaparrPhotosPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrOtherPath,
                originalValues[EnvKeys.ReaparrOtherPath]
            );
            System.Environment.SetEnvironmentVariable(
                EnvKeys.ReaparrGamesPath,
                originalValues[EnvKeys.ReaparrGamesPath]
            );
            System.Environment.SetEnvironmentVariable(EnvKeys.Home, originalValues[EnvKeys.Home]);
            System.Environment.SetEnvironmentVariable(EnvKeys.AppData, originalValues[EnvKeys.AppData]);
        }
    }
}

namespace Environment;

public class PathProvider : IPathProvider
{
    #region Properties

    #region DirectoryNames

    private static readonly string _configFolder = "Config";

    private static readonly string _logsFolder = "Logs";

    public static string DefaultRootSubDirectory => "PlexRipper";

    public static string DefaultMovieDestinationFolder => "Movies";

    public static string DefaultDownloadsDestinationFolder => "Downloads";

    public static string DefaultTvShowsDestinationFolder => "TvShows";

    public static string DefaultMusicDestinationFolder => "Music";

    public static string DefaultPhotosDestinationFolder => "Photos";

    public static string DefaultOtherDestinationFolder => "Other";

    public static string DefaultGamesDestinationFolder => "Games";

    #endregion

    #region FileNames

    public static string ConfigFileName => "PlexRipperSettings.json";

    public static string DatabaseName => "PlexRipperDB.db";
    public static string DatabaseShmName => $"{DatabaseName}-shm";
    public static string DatabaseWalName => $"{DatabaseName}-wal";

    #endregion

    public static string ConfigDirectory => Path.Combine(RootDirectory, _configFolder);

    public static string ConfigFileLocation => Path.Join(ConfigDirectory, ConfigFileName);

    public static string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

    public static string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

    public static string Database_SHM_Path => Path.Combine(ConfigDirectory, DatabaseShmName);

    public static string Database_WAL_Path => Path.Combine(ConfigDirectory, DatabaseWalName);

    public static string LogsDirectory => Path.Combine(RootDirectory, _configFolder, _logsFolder);

    public static string RootDirectory
    {
        get
        {
            var rootPath = EnvironmentExtensions.GetPlexRipperRootPathEnv();
            if (rootPath is not null)
                return rootPath;

            // Determine the root path based on the current OS
            switch (OsInfo.CurrentOS)
            {
                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.Osx:
                    return Path.Combine(EnvironmentExtensions.GetApplicationDirectoryPath(), DefaultRootSubDirectory);

                case OperatingSystemPlatform.Windows:
                    return Path.Combine(EnvironmentExtensions.GetApplicationDirectoryPath(), DefaultRootSubDirectory);
                default:
                    return "/";
            }
        }
    }

    public static string DatabaseConnectionString => $"Data Source={DatabasePath}";

    #region Interface Implementations

    string IPathProvider.RootDirectory => RootDirectory;

    string IPathProvider.ConfigFileLocation => ConfigFileLocation;

    string IPathProvider.ConfigFileName => ConfigFileName;

    string IPathProvider.DatabaseBackupDirectory => DatabaseBackupDirectory;

    string IPathProvider.DatabaseName => DatabaseName;

    string IPathProvider.DatabasePath => DatabasePath;

    string IPathProvider.Database_SHM_Path => Database_SHM_Path;

    string IPathProvider.Database_WAL_Path => Database_WAL_Path;

    string IPathProvider.LogsDirectory => LogsDirectory;

    string IPathProvider.ConfigDirectory => ConfigDirectory;

    public List<string> DatabaseFiles => new()
    {
        DatabasePath,
        Database_SHM_Path,
        Database_WAL_Path,
    };

    #endregion

    #endregion
}
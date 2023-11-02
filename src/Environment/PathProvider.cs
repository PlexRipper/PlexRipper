using System.Reflection;

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
            var devRootPath = EnvironmentExtensions.GetDevelopmentRootPath();
            if (devRootPath is not null)
                return devRootPath;

            string rootPath = "/";

            switch (OsInfo.CurrentOS)
            {
                case OperatingSystemPlatform.Linux:
                case OperatingSystemPlatform.Osx:

                    string PlexRipperEnvRootPath = EnvironmentExtensions.GetPlexRipperRootPath();
                    string HomeDirectory = EnvironmentExtensions.GetUserHomeDirectoryPath();

                    if (PlexRipperEnvRootPath is not null)
                    {
                        rootPath = PlexRipperEnvRootPath;
                        break;
                    }

                    if (HomeDirectory is not null)
                    {
                        rootPath = Path.Combine(HomeDirectory, DefaultRootSubDirectory);
                        break;
                    }

                    rootPath = Path.Combine("/", DefaultRootSubDirectory);
                    break;
                case OperatingSystemPlatform.Windows:
                    rootPath = Path.GetPathRoot(Assembly.GetExecutingAssembly().Location) ?? @"C:\";
                    break;
                default:
                    rootPath = "/";
                    break;
            }

            if (!System.IO.Directory.Exists(rootPath))
            {
                Directory.CreateDirectory(rootPath);
            }

            return rootPath;
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
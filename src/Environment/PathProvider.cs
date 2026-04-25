using Xdg.Directories;

namespace Reaparr.Environment;

public class PathProvider : IPathProvider
{
    private readonly IAppBuildInfo _appBuildInfo;

    public PathProvider(IAppBuildInfo? appBuildInfo = null)
    {
        _appBuildInfo = appBuildInfo ?? new AppBuildInfo();
    }

    #region Properties

    #region DirectoryNames

    public string DefaultLogsFolderName => "Logs";

    /// <inheritdoc/>
    public string DefaultConfigFolderName => "Config";

    /// <inheritdoc/>
    public string DefaultReaparrFolderName => "Reaparr";

    /// <inheritdoc/>
    public string DefaultMovieFolderName => "Movies";

    /// <inheritdoc/>
    public string DefaultDownloadsFolderName => "Downloads";

    /// <inheritdoc/>
    public string DefaultTvShowsFolderName => "TvShows";

    /// <inheritdoc/>
    public string DefaultMusicFolderName => "Music";

    /// <inheritdoc/>
    public string DefaultPhotosFolderName => "Photos";

    /// <inheritdoc/>
    public string DefaultOtherFolderName => "Other";

    /// <inheritdoc/>
    public string DefaultGamesFolderName => "Games";

    #region FileNames

    /// <inheritdoc/>
    public string ConfigFileName => "ReaparrSettings.json";

    /// <inheritdoc/>
    public string DatabaseName => "ReaparrDB.db";

    /// <inheritdoc/>
    public string DatabaseShmName => $"{DatabaseName}-shm";

    /// <inheritdoc/>
    public string DatabaseWalName => $"{DatabaseName}-wal";

    #endregion

    /// <inheritdoc/>
    public string DefaultDownloadsDestinationFolder
    {
        get
        {
            var downloadsPath = EnvironmentExtensions.GetDownloadsPath();
            if (downloadsPath is not null)
                return downloadsPath;

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultDownloadsFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultDownloadsFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string DefaultMovieDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetMoviesPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultMovieFolderName);

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultMovieFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultMovieFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string DefaultTvShowsDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetTvShowsPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultTvShowsFolderName);

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultTvShowsFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultTvShowsFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string DefaultMusicDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetMusicPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultMusicFolderName);

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultMusicFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultMusicFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string DefaultPhotosDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetPhotosPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultPhotosFolderName);

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultPhotosFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultPhotosFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string DefaultOtherDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetOtherPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultOtherFolderName);

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultOtherFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultOtherFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string DefaultGamesDestinationFolder
    {
        get
        {
            var perTypePath = EnvironmentExtensions.GetGamesPath();
            if (perTypePath is not null)
                return perTypePath;

            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return Path.Combine(dataPath, DefaultGamesFolderName);

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultGamesFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(DataDirectory, DefaultGamesFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    #endregion

    /// <inheritdoc/>
    public string ConfigDirectory
    {
        get
        {
            var configPath = EnvironmentExtensions.GetConfigPath();
            if (configPath != null)
                return configPath;

            if (_appBuildInfo.IsDockerMode)
                return Path.Combine("/", DefaultConfigFolderName);

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(BaseDirectory.ConfigHome, DefaultReaparrFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    /// <inheritdoc/>
    public string ConfigFileLocation => Path.Combine(ConfigDirectory, ConfigFileName);

    /// <inheritdoc/>
    public string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

    /// <inheritdoc/>
    public string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

    /// <inheritdoc/>
    // ReSharper disable once InconsistentNaming
    public string Database_SHM_Path => Path.Combine(ConfigDirectory, DatabaseShmName);

    /// <inheritdoc/>
    // ReSharper disable once InconsistentNaming
    public string Database_WAL_Path => Path.Combine(ConfigDirectory, DatabaseWalName);

    /// <inheritdoc/>
    public string LogsDirectory => Path.Combine(ConfigDirectory, DefaultLogsFolderName);

    /// <inheritdoc/>
    public List<string> DatabaseFiles => [DatabasePath, Database_SHM_Path, Database_WAL_Path];

    /// <inheritdoc/>
    public string DataDirectory
    {
        get
        {
            var dataPath = EnvironmentExtensions.GetDataPath();
            if (dataPath is not null)
                return dataPath;

            if (_appBuildInfo.IsDockerMode)
                return "/";

            if (_appBuildInfo.IsDesktopMode)
                return Path.Combine(UserDirectory.DownloadDir, DefaultReaparrFolderName);

            throw new PlatformNotSupportedException($"Platform: {_appBuildInfo.CurrentOS} is not supported");
        }
    }

    #endregion
}

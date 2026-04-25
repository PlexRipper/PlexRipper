namespace Reaparr.BaseTests;

public class MockPathProvider : IPathProvider
{
    private readonly string _sandboxFolder;
    private readonly IPathProvider _pathProvider;

    public MockPathProvider(string memoryDbName)
    {
        _pathProvider = new PathProvider(BaseUnitTest.CreateDefaultAppBuildInfo());
        _sandboxFolder = Path.GetFullPath(IntegrationTestFileSystemSandbox.GetSandboxFolder(memoryDbName));
    }

    /// <inheritdoc/>
    public string DefaultConfigFolderName => _pathProvider.DefaultConfigFolderName;

    /// <inheritdoc/>
    public string DefaultReaparrFolderName => _pathProvider.DefaultReaparrFolderName;

    /// <inheritdoc/>
    public string DefaultMovieFolderName => _pathProvider.DefaultMovieFolderName;

    /// <inheritdoc/>
    public string DefaultDownloadsFolderName => _pathProvider.DefaultDownloadsFolderName;

    /// <inheritdoc/>
    public string DefaultTvShowsFolderName => _pathProvider.DefaultTvShowsFolderName;

    /// <inheritdoc/>
    public string DefaultMusicFolderName => _pathProvider.DefaultMusicFolderName;

    /// <inheritdoc/>
    public string DefaultPhotosFolderName => _pathProvider.DefaultPhotosFolderName;

    /// <inheritdoc/>
    public string DefaultOtherFolderName => _pathProvider.DefaultOtherFolderName;

    /// <inheritdoc/>
    public string DefaultGamesFolderName => _pathProvider.DefaultGamesFolderName;

    /// <inheritdoc/>
    public string ConfigFileName => _pathProvider.ConfigFileName;

    /// <inheritdoc/>
    public string DatabaseName => _pathProvider.DatabaseName;

    /// <inheritdoc/>
    public string DatabaseShmName => _pathProvider.DatabaseShmName;

    /// <inheritdoc/>
    public string DatabaseWalName => _pathProvider.DatabaseWalName;

    /// <inheritdoc/>
    public string DefaultDownloadsDestinationFolder => Path.Combine(_sandboxFolder, DefaultDownloadsFolderName);

    /// <inheritdoc/>
    public string DefaultMovieDestinationFolder => Path.Combine(_sandboxFolder, DefaultMovieFolderName);

    /// <inheritdoc/>
    public string DefaultTvShowsDestinationFolder => Path.Combine(_sandboxFolder, DefaultTvShowsFolderName);

    /// <inheritdoc/>
    public string DefaultMusicDestinationFolder => Path.Combine(_sandboxFolder, DefaultMusicFolderName);

    /// <inheritdoc/>
    public string DefaultPhotosDestinationFolder => Path.Combine(_sandboxFolder, DefaultPhotosFolderName);

    /// <inheritdoc/>
    public string DefaultOtherDestinationFolder => Path.Combine(_sandboxFolder, DefaultOtherFolderName);

    /// <inheritdoc/>
    public string DefaultGamesDestinationFolder => Path.Combine(_sandboxFolder, DefaultGamesFolderName);

    /// <inheritdoc/>
    public string ConfigDirectory => Path.Combine(_sandboxFolder, DefaultConfigFolderName);

    /// <inheritdoc/>
    public string ConfigFileLocation => Path.Combine(ConfigDirectory, ConfigFileName);

    /// <inheritdoc/>
    public string DatabaseBackupDirectory => Path.Combine(ConfigDirectory, "Database BackUp");

    /// <inheritdoc/>
    public string DatabasePath => Path.Combine(ConfigDirectory, DatabaseName);

    /// <inheritdoc/>
    public string Database_SHM_Path => Path.Combine(ConfigDirectory, DatabaseShmName);

    /// <inheritdoc/>
    public string Database_WAL_Path => Path.Combine(ConfigDirectory, DatabaseWalName);

    /// <inheritdoc/>
    public string LogsDirectory => Path.Combine(ConfigDirectory, "Logs");

    /// <inheritdoc/>
    public List<string> DatabaseFiles => [DatabasePath, Database_SHM_Path, Database_WAL_Path];

    /// <inheritdoc/>
    public string DataDirectory => _sandboxFolder;
}

namespace Reaparr.Environment;

public interface IPathProvider
{
    string ConfigFileLocation { get; }

    string ConfigFileName { get; }

    string DatabaseBackupDirectory { get; }

    string DatabaseName { get; }

    string DatabasePath { get; }

    string LogsDirectory { get; }

    string RootDirectory { get; }

    List<string> DatabaseFiles { get; }

    /// <summary>
    /// Gets the default downloads destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultDownloadsDestinationFolder { get; }

    /// <summary>
    /// Gets the default movies destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultMovieDestinationFolder { get; }

    /// <summary>
    /// Gets the default TV shows destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultTvShowsDestinationFolder { get; }

    /// <summary>
    /// Gets the default music destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultMusicDestinationFolder { get; }

    /// <summary>
    /// Gets the default photos destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultPhotosDestinationFolder { get; }

    /// <summary>
    /// Gets the default uncategorized media destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultOtherDestinationFolder { get; }

    /// <summary>
    /// Gets the default games destination path based on the configured data root or the current platform fallback.
    /// </summary>
    string DefaultGamesDestinationFolder { get; }

    /// <summary>
    /// Gets the directory that stores Reaparr's own application state, such as settings, the SQLite database, backups, and logs.
    /// This is distinct from <see cref="DataDirectory"/>, which is the root location for user media content like Movies, TV shows, and Downloads.
    /// </summary>
    string ConfigDirectory { get; }

    /// <summary>
    /// Gets the root directory for user media data, where default destination folders such as Movies, TV shows, Music, and Downloads are created.
    /// This is distinct from <see cref="ConfigDirectory"/>, which stores Reaparr's internal application files rather than media content.
    /// </summary>
    string DataDirectory { get; }
}

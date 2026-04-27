namespace Reaparr.Environment;

/// <summary>
/// Provides runtime storage paths resolved from Reaparr environment variables.
/// Values map to <see cref="EnvKeys"/> keys and are <see langword="null"/> when
/// the corresponding environment variable is not set or contains only whitespace.
/// </summary>
public interface IAppRuntimeInfo
{
    /// <summary>
    /// Gets the process user ID (PUID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    int PUID { get; }

    /// <summary>
    /// Gets the process group ID (PGID) from the environment. Returns -1 when not set or invalid.
    /// </summary>
    int PGID { get; }

    /// <summary>
    /// Gets the GitHub token from <c>GITHUB_TOKEN</c> for authenticated GitHub API requests.
    /// </summary>
    string? GitHubToken { get; }

    /// <summary>
    /// Gets all defined System and user level Environment keys and their values
    /// </summary>
    Dictionary<string, string?> GetAllEnvironmentVariables { get; }

    #region Paths

    /// <summary>
    /// Gets the base data path from <see cref="EnvKeys.ReaparrDataPath"/>.
    /// </summary>
    string? DataPath { get; }

    /// <summary>
    /// Gets the default configuration path from <see cref="EnvKeys.ReaparrConfigPath"/>.
    /// </summary>
    string? ConfigPath { get; }

    /// <summary>
    /// Gets the default downloads path from <see cref="EnvKeys.ReaparrDownloadsPath"/>.
    /// </summary>
    string? DownloadsPath { get; }

    /// <summary>
    /// Gets the default movies library path from <see cref="EnvKeys.ReaparrMoviesPath"/>.
    /// </summary>
    string? MoviesPath { get; }

    /// <summary>
    /// Gets the default TV shows library path from <see cref="EnvKeys.ReaparrTvShowsPath"/>.
    /// </summary>
    string? TvShowsPath { get; }

    /// <summary>
    /// Gets the default music library path from <see cref="EnvKeys.ReaparrMusicPath"/>.
    /// </summary>
    string? MusicPath { get; }

    /// <summary>
    /// Gets the default photos library path from <see cref="EnvKeys.ReaparrPhotosPath"/>.
    /// </summary>
    string? PhotosPath { get; }

    /// <summary>
    /// Gets the miscellaneous library path from <see cref="EnvKeys.ReaparrOtherPath"/>.
    /// </summary>
    string? OtherPath { get; }

    /// <summary>
    /// Gets the default games library path from <see cref="EnvKeys.ReaparrGamesPath"/>.
    /// </summary>
    string? GamesPath { get; }

    string? AppImage { get; }

    /// <summary>
    /// When set to true, the application will log all environment variables set on startup
    /// </summary>
    bool ShouldLogEnvVars { get; }

    /// <summary>
    /// Sets the SEQ_URL environment variable to the specified URL.
    /// Note: This is used for development and testing purposes to redirect logs to a hosted Docker instance of Seq.
    /// </summary>
    string SEQ_Url { get; }

    #endregion
}
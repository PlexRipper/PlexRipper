namespace Reaparr.Environment;

/// <summary>
/// Provides runtime storage paths resolved from Reaparr environment variables.
/// Values map to <see cref="EnvKeys"/> keys and are <see langword="null"/> when
/// the corresponding environment variable is not set or contains only whitespace.
/// </summary>
public interface IAppRuntimeInfo
{
    /// <summary>
    /// Gets the GitHub token from <c>GITHUB_TOKEN</c> for authenticated GitHub API requests.
    /// </summary>
    string? GitHubToken { get; }

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

    #endregion
}
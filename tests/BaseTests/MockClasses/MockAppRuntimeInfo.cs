namespace Reaparr.BaseTests;

public class MockAppRuntimeInfo : IAppRuntimeInfo
{
    public MockAppRuntimeInfo() { }

    public int PUID { get; }
    public int PGID { get; }
    public string? GitHubToken { get; set; }

    public Dictionary<string, string?> GetAllEnvironmentVariables { get; } = new Dictionary<string, string?>();

    /// <inheritdoc/>
    public string? DataPath { get; }

    /// <inheritdoc/>
    public string? ConfigPath { get; }

    /// <inheritdoc/>
    public string? DownloadsPath { get; }

    /// <inheritdoc/>
    public string? MoviesPath { get; }

    /// <inheritdoc/>
    public string? TvShowsPath { get; }

    /// <inheritdoc/>
    public string? MusicPath { get; }

    /// <inheritdoc/>
    public string? PhotosPath { get; }

    /// <inheritdoc/>
    public string? OtherPath { get; }

    /// <inheritdoc/>
    public string? GamesPath { get; }

    public string? AppImage { get; }
    public bool ShouldLogEnvVars { get; }
    public string SEQ_Url { get; } = "http://localhost:5341";
}
namespace Reaparr.BaseTests;

public class MockAppRuntimeInfo : IAppRuntimeInfo
{
    public MockAppRuntimeInfo() { }

    public int PUID { get; set; }
    public int PGID { get; set; }
    public string? GitHubToken { get; set; }

    public Dictionary<string, string?> GetAllEnvironmentVariables { get; set; } = new Dictionary<string, string?>();

    /// <inheritdoc/>
    public string? DataPath { get; set; }

    /// <inheritdoc/>
    public string? ConfigPath { get; set; }

    /// <inheritdoc/>
    public string? DownloadsPath { get; set; }

    /// <inheritdoc/>
    public string? MoviesPath { get; set; }

    /// <inheritdoc/>
    public string? TvShowsPath { get; set; }

    /// <inheritdoc/>
    public string? MusicPath { get; set; }

    /// <inheritdoc/>
    public string? PhotosPath { get; set; }

    /// <inheritdoc/>
    public string? OtherPath { get; set; }

    /// <inheritdoc/>
    public string? GamesPath { get; set; }

    public string? AppImage { get; set; }
    public bool ShouldLogEnvVars { get; set; }
    public string SEQ_Url { get; set; } = "http://localhost:5341";
    public bool IsIntegrationTestMode { get; set; }

    public bool IsDevelopmentEnvironment { get; set; }

    public bool IsProductionEnvironment { get; set; }

    public string HeaderAuthTokenName { get; set; } = "X-Auth-User";
    public bool IsUnmasked { get; set; }
}

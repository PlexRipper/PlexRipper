namespace Reaparr.BaseTests;

public class MockAppRuntimeInfo : IAppRuntimeInfo
{
    public MockAppRuntimeInfo() { }

    public string? GitHubToken { get; set; }

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
}
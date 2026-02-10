using System.Diagnostics;

namespace Reaparr.External.Contracts;

/// <summary>
/// Interface for the dash-mpd-cli binary wrapper.
/// </summary>
public interface IDashMpdCliWrapper : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Gets whether the process is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the exit code of the process (only valid after process has exited).
    /// </summary>
    int? ExitCode { get; }

    /// <summary>
    /// Gets the task that completes when the process exits.
    /// </summary>
    Task<int> ProcessExitTask { get; }

    /// <summary>
    /// Event raised when standard output data is received.
    /// </summary>
    event DataReceivedEventHandler? OutputDataReceived;

    /// <summary>
    /// Event raised when standard error data is received.
    /// </summary>
    event DataReceivedEventHandler? ErrorDataReceived;

    /// <summary>
    /// Event raised when download progress is updated.
    /// </summary>
    event EventHandler<DownloadProgressEventArgs>? ProgressUpdated;

    /// <summary>
    /// Starts the dash-mpd-cli process with the specified arguments.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <returns>True if the process started successfully, false otherwise.</returns>
    bool Start(string mpdUrl, string outputPath, DashMpdCliOptions? options = null);

    /// <summary>
    /// Executes the dash-mpd-cli process and waits for it to complete.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The exit code of the process.</returns>
    Task<int> ExecuteAsync(
        string mpdUrl,
        string outputPath,
        DashMpdCliOptions? options = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stops the running process gracefully, or forcefully if it doesn't respond.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for graceful shutdown before forcing termination.</param>
    /// <returns>A task that completes when the process has stopped.</returns>
    Task StopAsync(TimeSpan? timeout = null);
}

/// <summary>
/// Configuration options for dash-mpd-cli execution.
/// </summary>
public class DashMpdCliOptions
{
    /// <summary>
    /// Gets or sets custom HTTP headers to include in requests.
    /// </summary>
    public Dictionary<string, string>? Headers { get; set; }

    /// <summary>
    /// Gets or sets the HTTP proxy to use.
    /// </summary>
    public string? Proxy { get; set; }

    /// <summary>
    /// Gets or sets whether to disable system proxy.
    /// </summary>
    public bool NoProxy { get; set; }

    /// <summary>
    /// Gets or sets the quality/resolution preference (e.g., "best", "worst", "1080p").
    /// </summary>
    public string? Quality { get; set; }

    /// <summary>
    /// Gets or sets whether to suppress output (quiet mode).
    /// </summary>
    public bool Quiet { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable verbose output.
    /// </summary>
    public bool Verbose { get; set; }

    /// <summary>
    /// Gets or sets the working directory for the process.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// Gets or sets custom environment variables for the process.
    /// </summary>
    public Dictionary<string, string>? EnvironmentVariables { get; set; }

    /// <summary>
    /// Gets or sets additional command-line arguments to pass to dash-mpd-cli.
    /// </summary>
    public string? AdditionalArguments { get; set; }

    #region Bandwidth Control

    /// <summary>
    /// Gets or sets network bandwidth throttling (e.g., "500K", "2M").
    /// </summary>
    public string? LimitRate { get; set; }

    #endregion

    #region Authentication

    /// <summary>
    /// Gets or sets HTTP Basic authentication username.
    /// </summary>
    public string? AuthUsername { get; set; }

    /// <summary>
    /// Gets or sets HTTP Basic authentication password.
    /// </summary>
    public string? AuthPassword { get; set; }

    /// <summary>
    /// Gets or sets Bearer token for authentication.
    /// </summary>
    public string? AuthBearer { get; set; }

    #endregion

    #region Advanced Options

    /// <summary>
    /// Gets or sets whether to enable pseudo-live stream support.
    /// </summary>
    public bool EnableLiveStreams { get; set; }

    /// <summary>
    /// Gets or sets sleep duration in milliseconds between requests.
    /// </summary>
    public int? SleepRequests { get; set; }

    /// <summary>
    /// Gets or sets browser name to extract cookies from (e.g., "firefox", "chrome", "edge", "safari").
    /// </summary>
    public string? CookiesFromBrowser { get; set; }

    #endregion

    #region Decryption (DRM)

    /// <summary>
    /// Gets or sets decryption keys for DRM-protected content.
    /// </summary>
    public List<string>? DecryptionKeys { get; set; }

    /// <summary>
    /// Gets or sets the decryption application to use ("mp4decrypt" or "shaka-packager").
    /// </summary>
    public string? DecryptionApplication { get; set; }

    /// <summary>
    /// Gets or sets custom path to mp4decrypt binary.
    /// </summary>
    public string? Mp4DecryptLocation { get; set; }

    /// <summary>
    /// Gets or sets custom path to shaka-packager binary.
    /// </summary>
    public string? ShakaPackagerLocation { get; set; }

    #endregion

    #region Muxing

    /// <summary>
    /// Gets or sets muxer preference mapping for different container formats (e.g., "mp4:ffmpeg,vlc").
    /// </summary>
    public Dictionary<string, string>? MuxerPreference { get; set; }

    /// <summary>
    /// Gets or sets custom path to ffmpeg binary.
    /// </summary>
    public string? FfmpegLocation { get; set; }

    /// <summary>
    /// Gets or sets custom path to VLC binary.
    /// </summary>
    public string? VlcLocation { get; set; }

    /// <summary>
    /// Gets or sets custom path to mkvmerge binary.
    /// </summary>
    public string? MkvmergeLocation { get; set; }

    /// <summary>
    /// Gets or sets custom path to MP4Box binary.
    /// </summary>
    public string? Mp4BoxLocation { get; set; }

    #endregion

    #region Quality Selection

    /// <summary>
    /// Gets or sets preferred video height in pixels (e.g., 1080).
    /// </summary>
    public int? PreferVideoHeight { get; set; }

    /// <summary>
    /// Gets or sets preferred video width in pixels (e.g., 1920).
    /// </summary>
    public int? PreferVideoWidth { get; set; }

    /// <summary>
    /// Gets or sets preferred audio language code (e.g., "en", "fr").
    /// </summary>
    public string? AudioLanguage { get; set; }

    #endregion

    #region Other

    /// <summary>
    /// Gets or sets XPath expression to drop elements from MPD manifest.
    /// </summary>
    public string? DropElements { get; set; }

    /// <summary>
    /// Gets or sets path to XSLT stylesheet for MPD manifest rewriting.
    /// </summary>
    public string? XsltStylesheet { get; set; }

    /// <summary>
    /// Gets or sets whether to disable concatenation of multi-period content.
    /// </summary>
    public bool NoPeriodConcatenation { get; set; }

    #endregion
}

/// <summary>
/// Event arguments for download progress updates.
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    /// <summary>
    /// Gets the percentage of download completed (0-100).
    /// </summary>
    public double PercentComplete { get; init; }

    /// <summary>
    /// Gets the number of bytes downloaded.
    /// </summary>
    public long BytesDownloaded { get; init; }

    /// <summary>
    /// Gets the total number of bytes to download (may be 0 if unknown).
    /// </summary>
    public long TotalBytes { get; init; }

    /// <summary>
    /// Gets the download speed in bytes per second.
    /// </summary>
    public long BytesPerSecond { get; init; }

    /// <summary>
    /// Gets the estimated time remaining in seconds (may be 0 if unknown).
    /// </summary>
    public int EstimatedSecondsRemaining { get; init; }

    /// <summary>
    /// Gets the raw output line that was parsed.
    /// </summary>
    public string RawOutput { get; init; } = string.Empty;
}

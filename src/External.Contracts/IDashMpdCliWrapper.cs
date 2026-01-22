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
}

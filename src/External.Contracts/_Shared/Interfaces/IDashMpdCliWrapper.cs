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
    /// Observable stream of standard output lines (including ANSI codes and carriage returns).
    /// </summary>
    IObservable<string> StandardOutput { get; }

    /// <summary>
    /// Observable stream of standard error lines.
    /// </summary>
    IObservable<string> StandardError { get; }

    /// <summary>
    /// Observable stream of parsed download progress updates.
    /// </summary>
    IObservable<DashDownloadProgress> Progress { get; }

    /// <summary>
    /// Starts the dash-mpd-cli process with the specified arguments.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <returns>Result.Ok if the process started successfully, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to start while a process is already running.</exception>
    Result Start(string mpdUrl, string outputPath, DashMpdCliOptions options);

    /// <summary>
    /// Executes the dash-mpd-cli process and waits for it to complete.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The exit code of the process.</returns>
    Task<Result> ExecuteAsync(
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

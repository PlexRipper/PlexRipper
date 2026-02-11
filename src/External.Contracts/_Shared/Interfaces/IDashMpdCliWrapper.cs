namespace Reaparr.External.Contracts;

/// <summary>
/// Interface for the dash-mpd-cli binary wrapper.
/// </summary>
public interface IDashMpdCliWrapper : IAsyncDisposable
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
    /// <param name="options">Optional configuration options for the download.</param>
    /// <returns>Result.Ok if the process started successfully, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to start while a process is already running.</exception>
    Task<Result> StartAsync(DashMpdCliOptions options);

    /// <summary>
    /// Stops the running process gracefully or forcefully if it doesn't respond.
    /// </summary>
    /// <returns>A task that completes when the process has stopped.</returns>
    Task StopAsync();
}

using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text.Json;
using CliWrap;
using CliWrap.EventStream;
using Reaparr.External.Contracts;

namespace Reaparr.External;

/// <summary>
/// Native wrapper around the dash-mpd-cli binary for downloading MPEG-DASH media.
/// Manages process lifecycle with start, stop, and clean disposal using CliWrap.
/// </summary>
public class DashMpdCliWrapper : IDashMpdCliWrapper
{
    private readonly ILogger _log;
    private readonly IFile _fileSystem;
    private readonly CancellationTokenSource _forcefulCts = new();
    private readonly CancellationTokenSource _gracefulCts = new();

    private readonly TaskCompletionSource<int> _processExitSource = new();
    private readonly string _binaryPath;

    private readonly Subject<string> _stdoutSubject = new();
    private readonly Subject<DashDownloadProgress> _progressSubject = new();

    private int? _exitCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashMpdCliWrapper"/> class.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    public DashMpdCliWrapper(ILogger logger, IFile fileSystem)
    {
        _log = logger.ForContext<DashMpdCliWrapper>();
        _fileSystem = fileSystem;
        _binaryPath = GetDefaultBinaryPath();
    }

    /// <summary>
    /// Gets the exit code of the process (only valid after process has exited).
    /// </summary>
    public int? ExitCode => _exitCode;

    /// <summary>
    /// Gets the task that completes when the process exits.
    /// </summary>
    public Task<int> ProcessExitTask => _processExitSource.Task;

    /// <summary>
    /// Observable stream of standard output lines (including ANSI codes and carriage returns).
    /// </summary>
    public IObservable<string> StandardOutput => _stdoutSubject.AsObservable();

    /// <summary>
    /// Observable stream of parsed download progress updates.
    /// </summary>
    public IObservable<DashDownloadProgress> Progress => _progressSubject.AsObservable();

    /// <inheritdoc/>
    public Task<Result> StartAsync(DashMpdCliOptions options)
    {
        if (!_fileSystem.Exists(_binaryPath))
            return Task.FromResult(
                _log.Here().ErrorResult($"dash-mpd-cli binary not found at: {_binaryPath}. Ensure the binary is included in the build output.")
            );

        if (string.IsNullOrWhiteSpace(options.MpdUrl))
            return Task.FromResult(_log.Here().ErrorResult("MPD URL cannot be null or empty"));

        if (string.IsNullOrWhiteSpace(options.Output))
            return Task.FromResult(_log.Here().ErrorResult("Output path cannot be null or empty"));

        var arguments = options.ToBuildArguments();

        _log.Here().Information("Starting dash-mpd-cli: {BinaryPath} {Arguments}", _binaryPath, arguments);
        _log.Here().Debug("Working directory: {WorkingDirectory}", options.WorkingDirectory);

        var envVars = options.EnvironmentVariables.ToDictionary(kvp => kvp.Key, string? (kvp) => kvp.Value);

        foreach (var (key, value) in options.EnvironmentVariables)
            _log.Here().Verbose("Environment variable: {Key}={Value}", key, value);

        var command = Cli.Wrap(_binaryPath)
            .WithValidation(CommandResultValidation.None)
            .WithArguments(arguments)
            .WithWorkingDirectory(options.WorkingDirectory)
            .WithEnvironmentVariables(envVars);

        // Run the event loop in a background task so StartAsync returns immediately
        Task.Run(() => RunEventLoopAsync(command));

        return Task.FromResult(Result.Ok());
    }

    private async Task RunEventLoopAsync(Command command)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_forcefulCts.Token, _gracefulCts.Token);

        try
        {
            await foreach (var cmdEvent in command.ListenAsync(linkedCts.Token))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        _log.Here().Information("dash-mpd-cli process started with PID {ProcessId}", started.ProcessId);
                        break;

                    // By convention for “data output” (what you want to pipe into another program or save to a file).
                    case StandardOutputCommandEvent stdOut:
                        HandleStdoutLine(stdOut.Text);
                        break;

                    // By convention “diagnostics” (logs, warnings, progress bars, info messages) happen in stderr
                    case StandardErrorCommandEvent stdErr:
                        HandleStdoutLine(stdErr.Text);
                        break;

                    case ExitedCommandEvent exited:
                        _exitCode = exited.ExitCode;
                        _log.Here().Information("dash-mpd-cli process exited with code {ExitCode}", exited.ExitCode);
                        break;
                }
            }

            _processExitSource.TrySetResult(_exitCode ?? -1);
        }
        catch (OperationCanceledException)
        {
            _log.Here().Debug("dash-mpd-cli event loop cancelled");
            _processExitSource.TrySetResult(_exitCode ?? -1);
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "dash-mpd-cli event loop faulted");
            _processExitSource.TrySetException(ex);
        }
        finally
        {
            CompleteObservables();
        }
    }

    private void HandleStdoutLine(string line)
    {
        if (string.IsNullOrEmpty(line))
            return;

        _stdoutSubject.OnNext(line);

        if (line.Contains("\"type\": \"progress\""))
        {
            var progress = TryParseProgress(line);
            if (progress != null)
            {
                _progressSubject.OnNext(progress);
            }
        }
        else
        {
            _log.Here().Debug("{Data}", line);
        }
    }

    /// <summary>
    /// Completes all observable subjects.
    /// </summary>
    private void CompleteObservables()
    {
        try
        {
            _stdoutSubject.OnCompleted();
            _progressSubject.OnCompleted();
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error completing observable subjects");
        }
    }

    /// <summary>
    /// Stops the running process gracefully, or forcefully if it doesn't respond.
    /// Also completes <see cref="ProcessExitTask"/> so callers don't hang if the event loop never started.
    /// </summary>
    /// <returns>A task that completes when the process has stopped.</returns>
    public async Task StopAsync()
    {
        _log.Here().Information("Stopping dash-mpd-cli process");
        await _gracefulCts.CancelAsync();
        _forcefulCts.CancelAfter(TimeSpan.FromSeconds(10));
        _processExitSource.TrySetResult(_exitCode ?? -1);
    }

    /// <summary>
    /// Asynchronously disposes of the wrapper and ensures the process is terminated.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        try
        {
            _stdoutSubject.Dispose();
            _progressSubject.Dispose();
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error disposing subjects");
        }
    }

    /// <summary>
    /// Attempts to parse progress information from dash-mpd-cli output.
    /// </summary>
    /// <param name="output">The output line to parse.</param>
    /// <returns>Progress information if successfully parsed, null otherwise.</returns>
    private DashDownloadProgress? TryParseProgress(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
            return null;

        var result = Result.Try(() =>
            JsonSerializer.Deserialize<DashProgressEvent>(output, DefaultJsonSerializerOptions.ConfigStandard)
        );

        if (result.IsFailed)
            return null;

        var valueEvent = result.Value;
        if (valueEvent is not { IsProgress: true })
            return null;

        return new DashDownloadProgress
        {
            ETA = TimeSpan.FromSeconds(valueEvent.EtaSeconds),
            Percent = valueEvent.Percent,
            DownloadSpeedInBytes = valueEvent.Bandwidth,
            CurrentStep = valueEvent.Message,
            RawOutput = output,
            DownloadedBytes = valueEvent.DownloadedBytes,
            TotalBytes = valueEvent.TotalBytes,
        };
    }

    private string GetDefaultBinaryPath()
    {
        var assemblyDir = AppContext.BaseDirectory;
        var binaryDir = Path.Combine(assemblyDir, "dash-mpd-cli", "binary");

        var binaryName = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "dash-mpd-cli-linux-amd64",
            Architecture.Arm64 => "dash-mpd-cli-linux-aarch64",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported architecture: {RuntimeInformation.OSArchitecture}. "
                    + $"Only x64 and ARM64 are supported."
            ),
        };

        return Path.Combine(binaryDir, binaryName);
    }
}

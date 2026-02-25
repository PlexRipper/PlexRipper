using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
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
    private readonly CancellationTokenSource _forcefulCts = new();
    private readonly CancellationTokenSource _gracefulCts = new();

    private readonly TaskCompletionSource<int> _processExitSource = new();
    private readonly string _binaryPath;

    private readonly Subject<string> _stdoutSubject = new();
    private readonly Subject<string> _stderrSubject = new();
    private readonly Subject<DashDownloadProgress> _progressSubject = new();

    private int? _exitCode;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashMpdCliWrapper"/> class.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the binary is not found at the expected location.</exception>
    public DashMpdCliWrapper(ILogger logger, IFile fileSystem)
    {
        _log = logger.ForContext<DashMpdCliWrapper>();
        _binaryPath = GetDefaultBinaryPath();

        if (!fileSystem.Exists(_binaryPath))
        {
            throw new FileNotFoundException(
                $"dash-mpd-cli binary not found at: {_binaryPath}. "
                    + $"Ensure the binary is included in the build output.",
                _binaryPath
            );
        }
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
    /// Observable stream of standard error lines.
    /// </summary>
    public IObservable<string> StandardError => _stderrSubject.AsObservable();

    /// <summary>
    /// Observable stream of parsed download progress updates.
    /// </summary>
    public IObservable<DashDownloadProgress> Progress => _progressSubject.AsObservable();

    /// <inheritdoc/>
    public async Task<Result> StartAsync(DashMpdCliOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.MpdUrl))
            return _log.Here().ErrorResult("MPD URL cannot be null or empty");

        if (string.IsNullOrWhiteSpace(options.Output))
            return _log.Here().ErrorResult("Output path cannot be null or empty");

        var arguments = options.ToBuildArguments(options.MpdUrl, options.Output);

        _log.Here().Information("Starting dash-mpd-cli: {BinaryPath} {Arguments}", _binaryPath, arguments);
        _log.Here().Debug("Working directory: {WorkingDirectory}", options.WorkingDirectory);

        try
        {
            var envVars = options.EnvironmentVariables.ToDictionary(kvp => kvp.Key, string? (kvp) => kvp.Value);

            foreach (var (key, value) in options.EnvironmentVariables)
                _log.Here().Verbose("Environment variable: {Key}={Value}", key, value);

            // Build the command using CliWrap's fluent API
            var command = Cli.Wrap(_binaryPath)
                .WithArguments(arguments)
                .WithWorkingDirectory(options.WorkingDirectory)
                .WithEnvironmentVariables(envVars);

            command
                .Observe()
                .Subscribe(
                    @event =>
                    {
                        switch (@event)
                        {
                            case StartedCommandEvent started:
                                _log.Here()
                                    .Information(
                                        "dash-mpd-cli process started successfully with PID {ProcessId}",
                                        started.ProcessId
                                    );
                                break;

                            case StandardOutputCommandEvent stdOut:
                                HandleStdoutLine(stdOut.Text);
                                break;

                            case StandardErrorCommandEvent stdErr:
                                HandleStderrLine(stdErr.Text);
                                break;

                            case ExitedCommandEvent exited:
                                _exitCode = exited.ExitCode;
                                _log.Here()
                                    .Information("dash-mpd-cli process exited with code {ExitCode}", exited.ExitCode);
                                _processExitSource.TrySetResult(exited.ExitCode);
                                CompleteObservables();
                                break;
                        }
                    },
                    err => _log.Here().Error(err, "Error during dash-mpd-cli execution")
                );

            // Start the execution task that processes the event stream
            await command.ExecuteAsync(_forcefulCts.Token, _gracefulCts.Token);

            _log.Here().Debug("dash-mpd-cli execution started with CliWrap");
            return Result.Ok();
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "Exception while starting dash-mpd-cli process");
            CleanupProcess();
            throw;
        }
    }

    /// <summary>
    /// Handles a stdout line and emits it to observables.
    /// </summary>
    private void HandleStdoutLine(string line)
    {
        try
        {
            if (string.IsNullOrEmpty(line))
                return;

            // Push to stdout observable
            _stdoutSubject.OnNext(line);
            _log.Here().Debug("{Data}", line);

            // Try to parse progress information
            var progress = TryParseProgress(line);
            if (progress != null)
            {
                _log.Here()
                    .Debug(
                        "Parsed progress: {Percent}%, {Speed}MB/s, {Step}",
                        progress.PercentComplete,
                        progress.DownloadSpeedMBps,
                        progress.CurrentStep
                    );
                _progressSubject.OnNext(progress);
            }
            else
            {
                _log.Here().Debug("{Data}", line);
                if (line.Contains(" INFO "))
                {
                    _log.Here().Information("{Data}", line);
                    return;
                }

                _log.Here().Debug("{Data}", line);
            }
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error processing stdout line: {Line}", line);
        }
    }

    /// <summary>
    /// Handles a stderr line and emits it to observables.
    /// </summary>
    private void HandleStderrLine(string line)
    {
        try
        {
            if (string.IsNullOrEmpty(line))
                return;

            _log.Here().Error("stderr: {Data}", line);

            // Push to stderr observable
            _stderrSubject.OnNext(line);
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error processing stderr line: {Line}", line);
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
            _stderrSubject.OnCompleted();
            _progressSubject.OnCompleted();
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error completing observable subjects");
        }
    }

    /// <summary>
    /// Stops the running process gracefully, or forcefully if it doesn't respond.
    /// </summary>
    /// <returns>A task that completes when the process has stopped.</returns>
    public async Task StopAsync()
    {
        _log.Here().Information("Stopping dash-mpd-cli process");
        await _gracefulCts.CancelAsync();
        _forcefulCts.CancelAfter(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Asynchronously disposes of the wrapper and ensures the process is terminated.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await StopAsync();

        CleanupProcess();
    }

    private void CleanupProcess()
    {
        _log.Here().Debug("Cleaning up dash-mpd-cli process");

        // Dispose subjects (OnCompleted should have been called already)
        try
        {
            _stdoutSubject.Dispose();
            _stderrSubject.Dispose();
            _progressSubject.Dispose();

            _log.Here().Debug("All subjects disposed successfully");
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
        {
            _log.Here().Error("Error parsing dash-mpd-cli process");
            result.LogError();
            return null;
        }

        var valueEvent = result.Value;
        if (valueEvent is not { IsProgress: true })
        {
            return null;
        }

        var speedMBps = valueEvent.Bandwidth > 0 ? valueEvent.Bandwidth / (1024.0 * 1024.0) : 0;

        return new DashDownloadProgress
        {
            ETA = TimeSpan.FromSeconds(valueEvent.EtaSeconds),
            PercentComplete = valueEvent.Percent,
            DownloadSpeedMBps = speedMBps,
            CurrentStep = valueEvent.Message,
            RawOutput = output,
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

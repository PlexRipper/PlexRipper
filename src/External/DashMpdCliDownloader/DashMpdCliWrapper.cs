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
    private readonly IFile _file;
    private readonly IDirectory _directory;
    private readonly CancellationTokenSource _forcefulCts = new();
    private readonly CancellationTokenSource _gracefulCts = new();
    private readonly CancellationTokenSource _linkedCts;
    private readonly string _binaryPath;

    private readonly Subject<string> _stdoutSubject = new();
    private readonly Subject<DashDownloadProgress> _progressSubject = new();

    private int? _exitCode;
    private string? _workingDirectory;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashMpdCliWrapper"/> class.
    /// </summary>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    public DashMpdCliWrapper(ILogger logger, IFile file, IDirectory directory)
    {
        _log = logger.ForContext<DashMpdCliWrapper>();
        _file = file;
        _directory = directory;
        _binaryPath = GetDefaultBinaryPath();
        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_forcefulCts.Token, _gracefulCts.Token);
    }

    /// <summary>
    /// Gets the exit code of the process (only valid after process has exited).
    /// </summary>
    public int? ExitCode => _exitCode;

    /// <summary>
    /// Observable stream of standard output lines (including ANSI codes and carriage returns).
    /// </summary>
    public IObservable<string> StandardOutput => _stdoutSubject.AsObservable();

    /// <summary>
    /// Observable stream of parsed download progress updates.
    /// </summary>
    public IObservable<DashDownloadProgress> Progress => _progressSubject.AsObservable();

    /// <inheritdoc/>
    public async Task<Result> StartAsync(DashMpdCliOptions options)
    {
        if (!_file.Exists(_binaryPath))
            return _log.Here()
                .ErrorResult(
                    $"dash-mpd-cli binary not found at: {_binaryPath}. Ensure the binary is included in the build output."
                );

        if (string.IsNullOrWhiteSpace(options.MpdUrl))
            return _log.Here().ErrorResult("MPD URL cannot be null or empty");

        if (string.IsNullOrWhiteSpace(options.Output))
            return _log.Here().ErrorResult("Output path cannot be null or empty");

        if (string.IsNullOrWhiteSpace(options.WorkingDirectory))
            return _log.Here().ErrorResult("Working directory cannot be null or empty");

        var arguments = options.ToBuildArguments();

        _log.Here().Information("Starting dash-mpd-cli: {BinaryPath} {Arguments}", _binaryPath, arguments);
        _log.Here().Debug("Working directory: {WorkingDirectory}", options.WorkingDirectory);

        var envVars = options.EnvironmentVariables.ToDictionary(kvp => kvp.Key, string? (kvp) => kvp.Value);

        foreach (var (key, value) in options.EnvironmentVariables)
            _log.Here().Verbose("Environment variable: {Key}={Value}", key, value);

        _workingDirectory = options.WorkingDirectory;

        var command = Cli.Wrap(_binaryPath)
            .WithValidation(CommandResultValidation.None)
            .WithArguments(arguments)
            .WithWorkingDirectory(options.WorkingDirectory)
            .WithEnvironmentVariables(envVars);

        return await RunEventLoopAsync(command, _linkedCts.Token);
    }

    private async Task<Result> RunEventLoopAsync(Command command, CancellationToken cancellationToken)
    {
        var listenResult = await Result.Try(async Task<IAsyncEnumerable<CommandEvent>> () =>
        {
            await foreach (var cmdEvent in command.ListenAsync(cancellationToken))
            {
                switch (cmdEvent)
                {
                    case StartedCommandEvent started:
                        _log.Here().Information("dash-mpd-cli process started with PID {ProcessId}", started.ProcessId);
                        break;

                    // By convention for "data output" (what you want to pipe into another program or save to a file).
                    case StandardOutputCommandEvent stdOut:
                        HandleStdoutLine(stdOut.Text);
                        break;

                    // By convention "diagnostics" (logs, warnings, progress bars, info messages) happen in stderr
                    case StandardErrorCommandEvent stdErr:
                        HandleStdoutLine(stdErr.Text);
                        break;

                    case ExitedCommandEvent exited:
                        _exitCode = exited.ExitCode;
                        _log.Here().Information("dash-mpd-cli process exited with code {ExitCode}", exited.ExitCode);
                        break;
                }
            }

            return Result.Ok();
        });

        var cleanUpResult = Result.Try(() =>
        {
            if (string.IsNullOrEmpty(_workingDirectory))
                return;

            try
            {
                foreach (var file in _directory.GetFiles(_workingDirectory, "dashmpd-*"))
                {
                    _file.Delete(file);
                    _log.Here().Debug("Deleted dash-mpd-cli temp file: {File}", file);
                }
            }
            catch (Exception ex)
            {
                _log.Here()
                    .Warning(ex, "Failed to clean up dash-mpd-cli temp files in {WorkingDirectory}", _workingDirectory);
                throw;
            }
        });

        return listenResult.ToResult();
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
    /// Stops the running process gracefully, or forcefully if it doesn't respond.
    /// </summary>
    /// <returns>A task that completes when the process has stopped.</returns>
    public async Task<Result> StopAsync()
    {
        _log.Here().Information("Stopping dash-mpd-cli process");
        await _gracefulCts.CancelAsync();
        _forcefulCts.CancelAfter(TimeSpan.FromSeconds(10));
        return Result.Ok();
    }

    /// <summary>
    /// Asynchronously disposes of the wrapper and ensures the process is terminated.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        try
        {
            _stdoutSubject.OnCompleted();
            _progressSubject.OnCompleted();

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

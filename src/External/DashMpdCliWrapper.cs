using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
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
    private readonly IFile _fileSystem;
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
        _fileSystem = fileSystem;
        _binaryPath = GetDefaultBinaryPath();

        if (!_fileSystem.Exists(_binaryPath))
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

        var arguments = BuildArguments(options.MpdUrl, options.Output, options);

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

            // dash-mpd-cli may output progress to stderr as well
            var progress = TryParseProgress(line);
            if (progress != null)
            {
                _log.Here().Verbose("Parsed progress from stderr: {Percent}%", progress.PercentComplete);
                _progressSubject.OnNext(progress);
            }
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

    private static string BuildArguments(string mpdUrl, string outputPath, DashMpdCliOptions options)
    {
        var args = new StringBuilder();

        // Output file
        args.Append($"--output \"{outputPath}\"");

        // Verbosity
        if (options.Quiet)
        {
            args.Append(" --quiet");
        }
        else if (options.Verbose)
        {
            args.Append(" -v -v");
        }

        // Custom headers
        if (options.Headers != null)
        {
            foreach (var (key, value) in options.Headers)
            {
                args.Append($" --add-header \"{key}: {value}\"");
            }
        }

        // Proxy configuration
        if (!string.IsNullOrWhiteSpace(options.Proxy))
        {
            args.Append($" --proxy \"{options.Proxy}\"");
        }

        if (options.NoProxy)
        {
            args.Append(" --no-proxy");
        }

        // Bandwidth control
        if (!string.IsNullOrWhiteSpace(options.LimitRate))
        {
            args.Append($" --limit-rate {options.LimitRate}");
        }

        // Authentication
        if (!string.IsNullOrWhiteSpace(options.AuthUsername))
        {
            args.Append($" --auth-username \"{options.AuthUsername}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthPassword))
        {
            args.Append($" --auth-password \"{options.AuthPassword}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.AuthBearer))
        {
            args.Append($" --auth-bearer \"{options.AuthBearer}\"");
        }

        // Advanced options
        if (options.EnableLiveStreams)
        {
            args.Append(" --enable-live-streams");
        }

        if (options.SleepRequests.HasValue)
        {
            args.Append($" --sleep-requests {options.SleepRequests.Value}");
        }

        if (!string.IsNullOrWhiteSpace(options.CookiesFromBrowser))
        {
            args.Append($" --cookies-from-browser {options.CookiesFromBrowser}");
        }

        // Decryption (DRM)
        if (options.DecryptionKeys != null)
        {
            foreach (var key in options.DecryptionKeys)
            {
                args.Append($" --key \"{key}\"");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.DecryptionApplication))
        {
            args.Append($" --decryption-application {options.DecryptionApplication}");
        }

        if (!string.IsNullOrWhiteSpace(options.Mp4DecryptLocation))
        {
            args.Append($" --mp4decrypt-location \"{options.Mp4DecryptLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.ShakaPackagerLocation))
        {
            args.Append($" --shaka-packager-location \"{options.ShakaPackagerLocation}\"");
        }

        // Muxing
        if (options.MuxerPreference != null)
        {
            foreach (var (container, preference) in options.MuxerPreference)
            {
                args.Append($" --muxer-preference {container}:{preference}");
            }
        }

        if (!string.IsNullOrWhiteSpace(options.FfmpegLocation))
        {
            args.Append($" --ffmpeg-location \"{options.FfmpegLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.VlcLocation))
        {
            args.Append($" --vlc-location \"{options.VlcLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.MkvmergeLocation))
        {
            args.Append($" --mkvmerge-location \"{options.MkvmergeLocation}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.Mp4BoxLocation))
        {
            args.Append($" --mp4box-location \"{options.Mp4BoxLocation}\"");
        }

        // Quality selection
        if (!string.IsNullOrWhiteSpace(options.Quality))
        {
            args.Append($" --quality \"{options.Quality}\"");
        }

        if (options.PreferVideoHeight.HasValue)
        {
            args.Append($" --prefer-video-height {options.PreferVideoHeight.Value}");
        }

        if (options.PreferVideoWidth.HasValue)
        {
            args.Append($" --prefer-video-width {options.PreferVideoWidth.Value}");
        }

        if (!string.IsNullOrWhiteSpace(options.AudioLanguage))
        {
            args.Append($" --prefer-language \"{options.AudioLanguage}\"");
        }

        // Other options
        if (!string.IsNullOrWhiteSpace(options.DropElements))
        {
            args.Append($" --drop-elements \"{options.DropElements}\"");
        }

        if (!string.IsNullOrWhiteSpace(options.XsltStylesheet))
        {
            args.Append($" --xslt-stylesheet \"{options.XsltStylesheet}\"");
        }

        if (options.NoPeriodConcatenation)
        {
            args.Append(" --no-period-concatenation");
        }

        // Additional custom arguments
        if (!string.IsNullOrWhiteSpace(options.AdditionalArguments))
        {
            args.Append($" {options.AdditionalArguments}");
        }

        // MPD URL (must be last)
        args.Append($" \"{mpdUrl}\"");

        return args.ToString();
    }

    /// <summary>
    /// Attempts to parse progress information from dash-mpd-cli output.
    /// Parses the format: [elapsed time] [percentage] [step description] [speed]
    /// </summary>
    /// <param name="output">The output line to parse.</param>
    /// <returns>Progress information if successfully parsed, null otherwise.</returns>
    private static DashDownloadProgress? TryParseProgress(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        // Try to extract percentage - this is the key indicator of progress
        var percentMatch = Regex.Match(output, @"(\d+\.?\d*)%");
        if (!percentMatch.Success || !double.TryParse(percentMatch.Groups[1].Value, out var percentage))
        {
            return null;
        }

        var progress = new DashDownloadProgress { PercentComplete = percentage, RawOutput = output };

        // Extract elapsed time - supports formats like "1m 30s", "00:01:30", "90s"
        var elapsedTime = TryParseElapsedTime(output);
        if (elapsedTime.HasValue)
        {
            progress = progress with { ElapsedTime = elapsedTime.Value };
        }

        // Extract download speed - supports formats like "2.5 MB/s", "1500 KB/s"
        var speedMBps = TryParseSpeed(output);
        if (speedMBps.HasValue)
        {
            progress = progress with { DownloadSpeedMBps = speedMBps.Value };
        }

        // Extract step/description - text between time and speed indicators
        var step = TryParseCurrentStep(output);
        if (!string.IsNullOrEmpty(step))
        {
            progress = progress with { CurrentStep = step };
        }

        return progress;
    }

    /// <summary>
    /// Parses elapsed time from various formats: "1m 30s", "00:01:30", "90s", etc.
    /// </summary>
    private static TimeSpan? TryParseElapsedTime(string output)
    {
        // Format: HH:MM:SS or MM:SS
        var timeMatch = Regex.Match(output, @"(\d{1,2}):(\d{2})(?::(\d{2}))?");
        if (timeMatch.Success)
        {
            var hours = timeMatch.Groups[3].Success ? int.Parse(timeMatch.Groups[1].Value) : 0;
            var minutes = timeMatch.Groups[3].Success
                ? int.Parse(timeMatch.Groups[2].Value)
                : int.Parse(timeMatch.Groups[1].Value);
            var seconds = timeMatch.Groups[3].Success
                ? int.Parse(timeMatch.Groups[3].Value)
                : int.Parse(timeMatch.Groups[2].Value);

            return new TimeSpan(hours, minutes, seconds);
        }

        // Format: "1h 30m 45s", "30m 45s", "45s"
        var componentMatch = Regex.Match(output, @"(?:(\d+)h\s*)?(?:(\d+)m\s*)?(?:(\d+)s)?");
        if (
            componentMatch.Success
            && (
                componentMatch.Groups[1].Success || componentMatch.Groups[2].Success || componentMatch.Groups[3].Success
            )
        )
        {
            var hours = componentMatch.Groups[1].Success ? int.Parse(componentMatch.Groups[1].Value) : 0;
            var minutes = componentMatch.Groups[2].Success ? int.Parse(componentMatch.Groups[2].Value) : 0;
            var seconds = componentMatch.Groups[3].Success ? int.Parse(componentMatch.Groups[3].Value) : 0;

            return new TimeSpan(hours, minutes, seconds);
        }

        return null;
    }

    /// <summary>
    /// Parses download speed and converts to MB/s.
    /// </summary>
    private static double? TryParseSpeed(string output)
    {
        var speedMatch = Regex.Match(output, @"(\d+\.?\d*)\s*(KB|MB|GB|kb|mb|gb)\/s", RegexOptions.IgnoreCase);
        if (speedMatch.Success && double.TryParse(speedMatch.Groups[1].Value, out var speed))
        {
            var unit = speedMatch.Groups[2].Value.ToUpperInvariant();
            return unit switch
            {
                "GB" => speed * 1024.0,
                "MB" => speed,
                "KB" => speed / 1024.0,
                _ => speed,
            };
        }

        return null;
    }

    /// <summary>
    /// Extracts the current step/operation description from the output.
    /// </summary>
    private static string TryParseCurrentStep(string output)
    {
        // Look for common step indicators
        var keywords = new[]
        {
            "Fetching",
            "Downloading",
            "Muxing",
            "Decrypting",
            "Processing",
            "Merging",
            "Converting",
            "Extracting",
            "Parsing",
            "Analyzing",
            "Preparing",
        };

        foreach (var keyword in keywords)
        {
            var index = output.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                // Extract a reasonable portion after the keyword
                var stepText = output[index..];
                // Take until we hit percentage or speed indicator
                var endMatch = Regex.Match(stepText, @"(\d+\.?\d*%|\d+\.?\d*\s*[KMG]B\/s)");
                if (endMatch.Success)
                {
                    stepText = stepText[..endMatch.Index].Trim();
                }

                return stepText.Length > 0 && stepText.Length < 100 ? stepText : keyword;
            }
        }

        return string.Empty;
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

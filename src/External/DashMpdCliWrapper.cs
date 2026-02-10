using System.Diagnostics;
using System.IO.Abstractions;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using Reaparr.External.Contracts;

namespace Reaparr.External;

/// <summary>
/// Native wrapper around the dash-mpd-cli binary for downloading MPEG-DASH media.
/// Manages process lifecycle with start, stop, and clean disposal.
/// </summary>
public class DashMpdCliWrapper : IDashMpdCliWrapper
{
    private readonly IFile _fileSystem;
    private readonly ILogger _log;
    private Process? _process;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TaskCompletionSource<int> _processExitSource = new();
    private readonly string _binaryPath;

    private Subject<string> _stdoutSubject = new();
    private Subject<string> _stderrSubject = new();
    private Subject<DashDownloadProgress> _progressSubject = new();

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
    /// Gets whether the process is currently running.
    /// </summary>
    public bool IsRunning => _process != null && !_process.HasExited;

    /// <summary>
    /// Gets the exit code of the process (only valid after process has exited).
    /// </summary>
    public int? ExitCode => _process?.HasExited == true ? _process.ExitCode : null;

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

    /// <summary>
    /// Starts the dash-mpd-cli process with the specified arguments.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <returns>True if the process started successfully, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to start while a process is already running.</exception>
    public Result Start(string mpdUrl, string outputPath, DashMpdCliOptions? options = null)
    {
        if (IsRunning)
            return _log.Here().ErrorResult("Cannot start dash-mpd-cli: process is already running.");

        if (string.IsNullOrWhiteSpace(mpdUrl))
            return _log.Here().ErrorResult("MPD URL cannot be null or empty");

        if (string.IsNullOrWhiteSpace(outputPath))
            return _log.Here().ErrorResult("Output path cannot be null or empty");

        options ??= new DashMpdCliOptions();

        var arguments = BuildArguments(mpdUrl, outputPath, options);

        _log.Here().Information("Starting dash-mpd-cli: {BinaryPath} {Arguments}", _binaryPath, arguments);
        _log.Here()
            .Debug(
                "Working directory: {WorkingDirectory}",
                options.WorkingDirectory ?? System.Environment.CurrentDirectory
            );

        var startInfo = new ProcessStartInfo
        {
            FileName = _binaryPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = false,
            CreateNoWindow = true,
            WorkingDirectory = options.WorkingDirectory ?? System.Environment.CurrentDirectory,
        };

        // Add custom environment variables if specified
        if (options.EnvironmentVariables != null)
        {
            foreach (var (key, value) in options.EnvironmentVariables)
            {
                startInfo.Environment[key] = value;
                _log.Here().Debug("Environment variable: {Key}={Value}", key, value);
            }
        }

        try
        {
            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            // Wire up event handlers
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.Exited += OnProcessExited;

            var started = _process.Start();

            if (started)
            {
                _log.Here().Information("dash-mpd-cli process started successfully with PID {ProcessId}", _process.Id);

                // Start reading stderr with line-based reading (errors are typically line-based)
                _process.BeginErrorReadLine();
                
                // Start reading stdout with custom reader to capture carriage return updates
                _ = Task.Run(async () => await ReadStdoutWithCarriageReturnSupport());

                _log.Here().Debug("Began asynchronous reading of output streams with carriage return support");
                return Result.Ok();
            }

            return _log.Here().ErrorResult("Failed to start dash-mpd-cli process");
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "Exception while starting dash-mpd-cli process");
            CleanupProcess();
            throw;
        }
    }

    /// <summary>
    /// Executes the dash-mpd-cli process and waits for it to complete.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The exit code of the process.</returns>
    public async Task<Result> ExecuteAsync(
        string mpdUrl,
        string outputPath,
        DashMpdCliOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            _cancellationTokenSource.Token
        );

        var startResult = Start(mpdUrl, outputPath, options);

        if (startResult.IsFailed)
            return startResult;

        try
        {
            // Wait for the process to exit or cancellation
            await ProcessExitTask.WaitAsync(linkedCts.Token);
            return Result.Ok();
        }
        catch (OperationCanceledException)
        {
            // Stop the process if cancelled
            await StopAsync();
            throw;
        }
    }

    /// <summary>
    /// Stops the running process gracefully, or forcefully if it doesn't respond.
    /// </summary>
    /// <param name="timeout">Maximum time to wait for graceful shutdown before forcing termination. Default is 5 seconds.</param>
    /// <returns>A task that completes when the process has stopped.</returns>
    public async Task StopAsync(TimeSpan? timeout = null)
    {
        if (!IsRunning)
            return;

        timeout ??= TimeSpan.FromSeconds(5);

        try
        {
            // Attempt a graceful shutdown by closing input
            if (_process?.StandardInput.BaseStream.CanWrite == true)
            {
                await _process.StandardInput.BaseStream.FlushAsync();
                _process.StandardInput.Close();
            }

            // Wait for graceful exit with timeout
            var exitTask = ProcessExitTask;
            var completedTask = await Task.WhenAny(exitTask, Task.Delay(timeout.Value));

            if (completedTask == exitTask)
            {
                // Process exited gracefully
                return;
            }

            // Force kill if still running
            if (_process?.HasExited == false)
            {
                _process.Kill(entireProcessTree: true);
                await _process.WaitForExitAsync();
            }
        }
        catch
        {
            // Ensure process is terminated even if exceptions occur
            try
            {
                if (_process?.HasExited == false)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // Ignore kill exceptions
            }
        }
    }

    /// <summary>
    /// Disposes of the wrapper and ensures the process is terminated.
    /// </summary>
    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Asynchronously disposes of the wrapper and ensures the process is terminated.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (IsRunning)
        {
            await StopAsync();
        }

        CleanupProcess();
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Reads stdout stream and splits on both newlines and carriage returns to capture progress updates.
    /// </summary>
    private async Task ReadStdoutWithCarriageReturnSupport()
    {
        if (_process?.StandardOutput == null)
        {
            _log.Here().Warning("Process stdout is null, cannot read output");
            return;
        }

        try
        {
            var buffer = new char[4096];
            var lineBuilder = new StringBuilder();

            while (!_process.HasExited)
            {
                var charsRead = await _process.StandardOutput.ReadAsync(buffer, 0, buffer.Length);
                
                if (charsRead == 0)
                    break;

                for (int i = 0; i < charsRead; i++)
                {
                    var ch = buffer[i];

                    if (ch == '\n')
                    {
                        // Newline - emit the line and clear buffer
                        var line = lineBuilder.ToString();
                        if (!string.IsNullOrEmpty(line))
                        {
                            EmitStdoutLine(line);
                        }
                        lineBuilder.Clear();
                    }
                    else if (ch == '\r')
                    {
                        // Carriage return - emit the current line (progress update) and clear
                        var line = lineBuilder.ToString();
                        if (!string.IsNullOrEmpty(line))
                        {
                            EmitStdoutLine(line);
                        }
                        lineBuilder.Clear();
                    }
                    else
                    {
                        lineBuilder.Append(ch);
                    }
                }
            }

            // Emit any remaining content
            if (lineBuilder.Length > 0)
            {
                EmitStdoutLine(lineBuilder.ToString());
            }

            _log.Here().Debug("Finished reading stdout stream");
        }
        catch (Exception ex)
        {
            _log.Here().Error(ex, "Error reading stdout stream");
            _stdoutSubject?.OnError(ex);
        }
    }

    /// <summary>
    /// Emits a line to the stdout subject and attempts to parse progress.
    /// </summary>
    private void EmitStdoutLine(string line)
    {
        try
        {
            // Push to stdout observable
            _stdoutSubject?.OnNext(line);

            // Try to parse progress information
            var progress = TryParseProgress(line);
            if (progress != null)
            {
                _log.Here().Verbose("Parsed progress: {Percent}%, {Speed}MB/s, {Step}", 
                    progress.PercentComplete, progress.DownloadSpeedMBps, progress.CurrentStep);
                _progressSubject?.OnNext(progress);
            }
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error emitting stdout line: {Line}", line);
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (e.Data != null)
        {
            try
            {
                _log.Here().Error("stderr: {Data}", e.Data);

                // Push all error output to stderr observable
                _stderrSubject.OnNext(e.Data);

                // dash-mpd-cli may output progress to stderr as well
                var progress = TryParseProgress(e.Data);
                if (progress != null)
                {
                    _log.Here().Verbose("Parsed progress from stderr: {Percent}%", progress.PercentComplete);
                    _progressSubject.OnNext(progress);
                }
            }
            catch (Exception ex)
            {
                _log.Here().Warning(ex, "Error processing stderr data: {Data}", e.Data);
            }
        }
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        var exitCode = _process?.ExitCode ?? -1;

        _log.Here().Information("dash-mpd-cli process exited with code {ExitCode}", exitCode);

        if (_process != null)
        {
            _processExitSource.TrySetResult(exitCode);
        }

        // Complete all observables when process exits
        try
        {
            _stdoutSubject?.OnCompleted();
            _stderrSubject?.OnCompleted();
            _progressSubject?.OnCompleted();
            _log.Here().Debug("All observable subjects completed");
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Error completing observable subjects");
        }
    }

    private void CleanupProcess()
    {
        _log.Here().Debug("Cleaning up dash-mpd-cli process");

        if (_process != null)
        {
            try
            {
                _process.ErrorDataReceived -= OnErrorDataReceived;
                _process.Exited -= OnProcessExited;

                _process.Dispose();
                _process = null;
                _log.Here().Debug("Process disposed successfully");
            }
            catch (Exception ex)
            {
                _log.Here().Warning(ex, "Error disposing process");
            }
        }

        // Dispose subjects (OnCompleted should have been called already in OnProcessExited)
        try
        {
            _stdoutSubject?.Dispose();
            _stderrSubject?.Dispose();
            _progressSubject?.Dispose();

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
            args.Append(" --verbose");
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

        var binaryPath = Path.Combine(binaryDir, binaryName);

        // Ensure binary has execute permissions on Unix systems
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (_fileSystem.Exists(binaryPath))
            {
                try
                {
                    // Set execute permissions (equivalent to chmod +x)
                    EnsureExecutePermissions(binaryPath);
                }
                catch
                {
                    // If setting permissions fails, continue anyway
                    // The binary might already have execute permissions
                }
            }
        }

        return binaryPath;
    }

    /// <summary>
    /// Ensures the specified file has execute permissions on Unix systems.
    /// </summary>
    private static void EnsureExecutePermissions(string filePath)
    {
        var chmod = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x \"{filePath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            },
        };

        chmod.Start();
        chmod.WaitForExit();

        if (chmod.ExitCode != 0)
        {
            var error = chmod.StandardError.ReadToEnd();
            throw new InvalidOperationException($"Failed to set execute permissions: {error}");
        }
    }
}

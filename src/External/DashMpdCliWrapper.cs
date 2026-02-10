using System.Diagnostics;
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
    private Process? _process;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly TaskCompletionSource<int> _processExitSource = new();
    private readonly string _binaryPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashMpdCliWrapper"/> class.
    /// </summary>
    /// <param name="customBinaryPath">Optional custom path to dash-mpd-cli binary. If null, auto-detects based on platform.</param>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the binary is not found at the expected location.</exception>
    public DashMpdCliWrapper(string? customBinaryPath = null)
    {
        _binaryPath = customBinaryPath ?? GetDefaultBinaryPath();

        if (!File.Exists(_binaryPath))
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
    /// Event raised when standard output data is received.
    /// </summary>
    public event DataReceivedEventHandler? OutputDataReceived;

    /// <summary>
    /// Event raised when standard error data is received.
    /// </summary>
    public event DataReceivedEventHandler? ErrorDataReceived;

    /// <summary>
    /// Event raised when download progress is updated.
    /// </summary>
    public event EventHandler<DownloadProgressEventArgs>? ProgressUpdated;

    /// <summary>
    /// Starts the dash-mpd-cli process with the specified arguments.
    /// </summary>
    /// <param name="mpdUrl">The URL of the MPD manifest to download.</param>
    /// <param name="outputPath">The output file path where the downloaded media will be saved.</param>
    /// <param name="options">Optional configuration options for the download.</param>
    /// <returns>True if the process started successfully, false otherwise.</returns>
    /// <exception cref="InvalidOperationException">Thrown when attempting to start while a process is already running.</exception>
    public bool Start(string mpdUrl, string outputPath, DashMpdCliOptions? options = null)
    {
        if (IsRunning)
        {
            throw new InvalidOperationException("Cannot start dash-mpd-cli: process is already running.");
        }

        if (string.IsNullOrWhiteSpace(mpdUrl))
        {
            throw new ArgumentException("MPD URL cannot be null or empty.", nameof(mpdUrl));
        }

        if (string.IsNullOrWhiteSpace(outputPath))
        {
            throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));
        }

        options ??= new DashMpdCliOptions();

        var arguments = BuildArguments(mpdUrl, outputPath, options);

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
            }
        }

        try
        {
            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            // Wire up event handlers
            _process.OutputDataReceived += OnOutputDataReceived;
            _process.ErrorDataReceived += OnErrorDataReceived;
            _process.Exited += OnProcessExited;

            var started = _process.Start();

            if (started)
            {
                // Begin asynchronous reading of output streams
                _process.BeginOutputReadLine();
                _process.BeginErrorReadLine();
            }

            return started;
        }
        catch
        {
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
    public async Task<int> ExecuteAsync(
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

        if (!Start(mpdUrl, outputPath, options))
        {
            throw new InvalidOperationException("Failed to start dash-mpd-cli process.");
        }

        try
        {
            // Wait for process to exit or cancellation
            var exitCode = await ProcessExitTask.WaitAsync(linkedCts.Token);
            return exitCode;
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
        {
            return;
        }

        timeout ??= TimeSpan.FromSeconds(5);

        try
        {
            // Attempt graceful shutdown by closing input
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

    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            // Try to parse progress information
            var progress = TryParseProgress(e.Data);
            if (progress != null)
            {
                ProgressUpdated?.Invoke(this, progress);
            }
        }

        OutputDataReceived?.Invoke(sender, e);
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(e.Data))
        {
            // dash-mpd-cli may output progress to stderr as well
            var progress = TryParseProgress(e.Data);
            if (progress != null)
            {
                ProgressUpdated?.Invoke(this, progress);
            }
        }

        ErrorDataReceived?.Invoke(sender, e);
    }

    private void OnProcessExited(object? sender, EventArgs e)
    {
        if (_process != null)
        {
            _processExitSource.TrySetResult(_process.ExitCode);
        }
    }

    private void CleanupProcess()
    {
        if (_process != null)
        {
            _process.OutputDataReceived -= OnOutputDataReceived;
            _process.ErrorDataReceived -= OnErrorDataReceived;
            _process.Exited -= OnProcessExited;

            _process.Dispose();
            _process = null;
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
    /// Supports various output formats from the tool.
    /// </summary>
    /// <param name="output">The output line to parse.</param>
    /// <returns>Progress information if successfully parsed, null otherwise.</returns>
    private static DownloadProgressEventArgs? TryParseProgress(string output)
    {
        if (string.IsNullOrWhiteSpace(output))
        {
            return null;
        }

        // Pattern 1: Percentage-based progress (e.g., "Progress: 45.2%")
        var percentMatch = Regex.Match(output, @"(\d+\.?\d*)%", RegexOptions.IgnoreCase);
        if (percentMatch.Success && double.TryParse(percentMatch.Groups[1].Value, out var percent))
        {
            return new DownloadProgressEventArgs { PercentComplete = percent, RawOutput = output };
        }

        // Pattern 2: Downloaded bytes (e.g., "Downloaded 1.5GB / 3.0GB")
        var bytesMatch = Regex.Match(
            output,
            @"(\d+\.?\d*)\s*(KB|MB|GB|TB)?\s*\/\s*(\d+\.?\d*)\s*(KB|MB|GB|TB)?",
            RegexOptions.IgnoreCase
        );
        if (bytesMatch.Success)
        {
            var downloaded = ParseSize(bytesMatch.Groups[1].Value, bytesMatch.Groups[2].Value);
            var total = ParseSize(bytesMatch.Groups[3].Value, bytesMatch.Groups[4].Value);

            if (downloaded > 0 && total > 0)
            {
                var percentComplete = (double)downloaded / total * 100.0;
                return new DownloadProgressEventArgs
                {
                    PercentComplete = percentComplete,
                    BytesDownloaded = downloaded,
                    TotalBytes = total,
                    RawOutput = output,
                };
            }
        }

        // Pattern 3: Speed information (e.g., "Speed: 2.5 MB/s" or "1.2MB/s")
        var speedMatch = Regex.Match(output, @"(\d+\.?\d*)\s*(KB|MB|GB)\/s", RegexOptions.IgnoreCase);
        if (speedMatch.Success)
        {
            var speed = ParseSize(speedMatch.Groups[1].Value, speedMatch.Groups[2].Value);
            return new DownloadProgressEventArgs { BytesPerSecond = speed, RawOutput = output };
        }

        // Pattern 4: ETA/Time remaining (e.g., "ETA: 2m 30s" or "Remaining: 00:05:30")
        var etaMatch = Regex.Match(
            output,
            @"(?:ETA|Remaining):\s*(?:(\d+)h\s*)?(?:(\d+)m\s*)?(?:(\d+)s?)?|(\d{2}):(\d{2}):(\d{2})",
            RegexOptions.IgnoreCase
        );
        if (etaMatch.Success)
        {
            int seconds = 0;
            if (!string.IsNullOrEmpty(etaMatch.Groups[4].Value)) // HH:MM:SS format
            {
                seconds =
                    int.Parse(etaMatch.Groups[4].Value) * 3600
                    + int.Parse(etaMatch.Groups[5].Value) * 60
                    + int.Parse(etaMatch.Groups[6].Value);
            }
            else // Individual components
            {
                if (!string.IsNullOrEmpty(etaMatch.Groups[1].Value))
                    seconds += int.Parse(etaMatch.Groups[1].Value) * 3600;
                if (!string.IsNullOrEmpty(etaMatch.Groups[2].Value))
                    seconds += int.Parse(etaMatch.Groups[2].Value) * 60;
                if (!string.IsNullOrEmpty(etaMatch.Groups[3].Value))
                    seconds += int.Parse(etaMatch.Groups[3].Value);
            }

            return new DownloadProgressEventArgs { EstimatedSecondsRemaining = seconds, RawOutput = output };
        }

        return null;
    }

    /// <summary>
    /// Parses a size value with optional unit (KB, MB, GB, TB) and returns bytes.
    /// </summary>
    private static long ParseSize(string value, string unit)
    {
        if (!double.TryParse(value, out var size))
        {
            return 0;
        }

        return unit.ToUpperInvariant() switch
        {
            "TB" => (long)(size * 1024 * 1024 * 1024 * 1024),
            "GB" => (long)(size * 1024 * 1024 * 1024),
            "MB" => (long)(size * 1024 * 1024),
            "KB" => (long)(size * 1024),
            _ => (long)size,
        };
    }

    private static string GetDefaultBinaryPath()
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
            if (File.Exists(binaryPath))
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

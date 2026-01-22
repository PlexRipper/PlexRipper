using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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
                $"dash-mpd-cli binary not found at: {_binaryPath}. " +
                $"Ensure the binary is included in the build output.",
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
            WorkingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory,
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
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _cancellationTokenSource.Token);

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
        OutputDataReceived?.Invoke(sender, e);
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
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

        // Proxy
        if (!string.IsNullOrWhiteSpace(options.Proxy))
        {
            args.Append($" --proxy \"{options.Proxy}\"");
        }

        // Quality selection
        if (!string.IsNullOrWhiteSpace(options.Quality))
        {
            args.Append($" --quality \"{options.Quality}\"");
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

    private static string GetDefaultBinaryPath()
    {
        var assemblyDir = AppContext.BaseDirectory;
        var binaryDir = Path.Combine(assemblyDir, "dash-mpd-cli", "binary");

        var binaryName = RuntimeInformation.OSArchitecture switch
        {
            Architecture.X64 => "dash-mpd-cli-linux-amd64",
            Architecture.Arm64 => "dash-mpd-cli-linux-aarch64",
            _ => throw new PlatformNotSupportedException(
                $"Unsupported architecture: {RuntimeInformation.OSArchitecture}. " +
                $"Only x64 and ARM64 are supported."
            )
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
                    var fileInfo = new UnixFileInfo(binaryPath);
                    fileInfo.FileAccessPermissions |= FileAccessPermissions.UserExecute |
                                                       FileAccessPermissions.GroupExecute |
                                                       FileAccessPermissions.OtherExecute;
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
}

/// <summary>
/// Unix file information helper for setting file permissions on Linux/macOS.
/// </summary>
internal class UnixFileInfo
{
    private readonly string _path;

    public UnixFileInfo(string path)
    {
        _path = path;
    }

    public FileAccessPermissions FileAccessPermissions
    {
        get => throw new NotImplementedException("Reading permissions not implemented");
        set
        {
            // Use chmod system call via Process
            var chmod = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{_path}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            chmod.Start();
            chmod.WaitForExit();
        }
    }
}

[Flags]
internal enum FileAccessPermissions
{
    UserExecute = 0x40,
    GroupExecute = 0x08,
    OtherExecute = 0x01,
}

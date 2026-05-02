using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopLaunchWorkflow
{
    private readonly BuildPaths _paths;
    private readonly DesktopRuntime _runtime;
    private readonly DesktopCommandSettings _settings;
    private readonly IDesktopCommandRunner _commandRunner;
    private readonly DesktopPackageWorkflow _packageWorkflow;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DesktopLaunchWorkflow> _logger;
    public DesktopLaunchWorkflow(BuildPaths paths,
        DesktopRuntime runtime,
        DesktopCommandSettings settings,
        IDesktopCommandRunner commandRunner,
        DesktopPackageWorkflow packageWorkflow,
        IFileSystem fileSystem,
        ILogger<DesktopLaunchWorkflow> logger)
    {
        _paths = paths;
        _runtime = runtime;
        _settings = settings;
        _commandRunner = commandRunner;
        _packageWorkflow = packageWorkflow;
        _fileSystem = fileSystem;
        _logger = logger;
    }
    private const string LAUNCH_MODE_PACKAGED = "packaged";
    private const string LAUNCH_MODE_PUBLISHED = "published";

    public async Task<int> LaunchAsync()
    {
        if (
            _runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase)
            && !_settings.SkipPackage
            && string.Equals(_settings.LaunchMode, LAUNCH_MODE_PACKAGED, StringComparison.OrdinalIgnoreCase)
        )
        {
            var appImagePath = FindLinuxAppImage();
            _logger.LogInformation(
                "Launching packaged Linux AppImage for {RuntimeIdentifier} from {AppImagePath}",
                _runtime.RuntimeIdentifier,
                appImagePath
            );

            await _commandRunner.RunCommandAsync("chmod", ["+x", appImagePath]);

            var appImageExitCode = await _commandRunner.ExecuteCommandAsync(appImagePath, []);
            _logger.LogInformation(
                "Packaged Linux AppImage for {RuntimeIdentifier} exited with code {ExitCode}",
                _runtime.RuntimeIdentifier,
                appImageExitCode
            );

            return appImageExitCode;
        }

        if (
            _runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase)
            && !_settings.SkipPackage
            && !string.Equals(_settings.LaunchMode, LAUNCH_MODE_PUBLISHED, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(_settings.LaunchMode, LAUNCH_MODE_PACKAGED, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ArgumentException(
                $"Unsupported launch mode '{_settings.LaunchMode}'. Supported values are '{LAUNCH_MODE_PUBLISHED}' and '{LAUNCH_MODE_PACKAGED}'.",
                nameof(_settings.LaunchMode)
            );
        }

        var publishedExecutable = _fileSystem.Path.Combine(
            _paths.PublishDirectory(_runtime.RuntimeIdentifier),
            _runtime.MainExecutable
        );

        _logger.LogInformation(
            "Resolved published executable for {RuntimeIdentifier} to {PublishedExecutable} (launch mode: {LaunchMode})",
            _runtime.RuntimeIdentifier,
            publishedExecutable,
            _settings.LaunchMode
        );

        if (!_fileSystem.File.Exists(publishedExecutable))
        {
            throw new FileNotFoundException(
                $"Published executable for runtime '{_runtime.RuntimeIdentifier}' was not found at '{publishedExecutable}'.",
                publishedExecutable
            );
        }

        if (
            _runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase)
            && !OperatingSystem.IsWindows()
        )
        {
            if (await _commandRunner.CommandExistsAsync("wine"))
            {
                _logger.LogInformation(
                    "Launching Windows desktop build for {RuntimeIdentifier} with wine using {PublishedExecutable}",
                    _runtime.RuntimeIdentifier,
                    publishedExecutable
                );

                var wineExitCode = await _commandRunner.ExecuteCommandAsync("wine", [publishedExecutable]);
                _logger.LogInformation(
                    "Wine launch for {RuntimeIdentifier} exited with code {ExitCode}",
                    _runtime.RuntimeIdentifier,
                    wineExitCode
                );

                return wineExitCode;
            }

            _logger.LogInformation(
                "Published {RuntimeIdentifier} build to {PublishedExecutable}",
                _runtime.RuntimeIdentifier,
                publishedExecutable
            );
            _logger.LogInformation(
                "Packaged {RuntimeIdentifier} artifacts to {ArtifactDirectory}",
                _runtime.RuntimeIdentifier,
                _packageWorkflow.GetArtifactDirectory()
            );
            _logger.LogWarning("Wine is not available on this host, so the Windows build was not launched.");
            return 0;
        }

        _logger.LogInformation(
            "Launching published executable for {RuntimeIdentifier} directly from {PublishedExecutable}",
            _runtime.RuntimeIdentifier,
            publishedExecutable
        );

        var exitCode = await _commandRunner.ExecuteCommandAsync(publishedExecutable, []);
        _logger.LogInformation(
            "Published executable for {RuntimeIdentifier} exited with code {ExitCode}",
            _runtime.RuntimeIdentifier,
            exitCode
        );

        return exitCode;
    }
 

    private string FindLinuxAppImage()
    {
        var artifactDirectory = _packageWorkflow.GetArtifactDirectory();
        _logger.LogInformation(
            "Searching for packaged Linux AppImage for {RuntimeIdentifier} in {ArtifactDirectory}",
            _runtime.RuntimeIdentifier,
            artifactDirectory
        );

        if (!_fileSystem.Directory.Exists(artifactDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Artifact directory for runtime '{_runtime.RuntimeIdentifier}' was not found at '{artifactDirectory}'."
            );
        }

        var appImage = _fileSystem.Directory
            .EnumerateFiles(artifactDirectory, "*.AppImage", SearchOption.TopDirectoryOnly)
            .Select(file => _fileSystem.FileInfo.New(file))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (appImage is null)
        {
            throw new FileNotFoundException(
                $"No AppImage artifact was produced for runtime '{_runtime.RuntimeIdentifier}' in '{artifactDirectory}'."
            );
        }

        _logger.LogInformation(
            "Selected Linux AppImage for {RuntimeIdentifier}: {AppImagePath}",
            _runtime.RuntimeIdentifier,
            appImage.FullName
        );

        return appImage.FullName;
    }
}

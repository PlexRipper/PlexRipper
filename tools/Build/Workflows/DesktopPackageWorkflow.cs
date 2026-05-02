using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopPackageWorkflow
{
    private readonly BuildPaths _paths;
    private readonly DesktopRuntime _runtime;
    private readonly DesktopCommandSettings _settings;
    private readonly IDesktopCommandRunner _commandRunner;
    private readonly FileSystemTasks _fileSystemTasks;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DesktopPackageWorkflow> _logger;
    
    public DesktopPackageWorkflow(BuildPaths paths,
        DesktopRuntime runtime,
        DesktopCommandSettings settings,
        IDesktopCommandRunner commandRunner,
        FileSystemTasks fileSystemTasks,
        IFileSystem fileSystem,
        ILogger<DesktopPackageWorkflow> logger)
    {
        _paths = paths;
        _runtime = runtime;
        _settings = settings;
        _commandRunner = commandRunner;
        _fileSystemTasks = fileSystemTasks;
        _fileSystem = fileSystem;
        _logger = logger;
    }
    private const string PACKAGE_ID = "Reaparr";
    private const string PACKAGE_TITLE = "Reaparr";

    public async Task PackageAsync()
    {
        if (!_settings.DryRun)
        {
            if (!_settings.PreserveExistingArtifacts)
            {
                _fileSystemTasks.ClearArtifactDirectory(_paths.RootDirectory, GetArtifactDirectory());
            }
            else
            {
                _fileSystem.Directory.CreateDirectory(GetArtifactDirectory());
            }
        }

        await _commandRunner.RunCommandAsync("vpk", CreatePackArguments(_paths, _runtime, _settings, GetArtifactDirectory()));

        _logger.LogInformation(
            "Packaged {RuntimeIdentifier} desktop artifacts for Velopack channel {Channel} to {ArtifactDirectory}",
            _runtime.RuntimeIdentifier,
            GetChannel(),
            GetArtifactDirectory()
        );
    }

    public string GetArtifactDirectory() =>
        string.IsNullOrWhiteSpace(_settings.ArtifactDirectory)
            ? _paths.ArtifactDirectory(_runtime.RuntimeIdentifier)
            : _fileSystem.Path.GetFullPath(_settings.ArtifactDirectory, _paths.RootDirectory);

    private static List<string> CreatePackArguments(
        BuildPaths paths,
        DesktopRuntime runtime,
        DesktopCommandSettings settings,
        string? artifactDirectory
    ) =>
        [
            "pack",
            "--packId",
            PACKAGE_ID,
            "--packTitle",
            PACKAGE_TITLE,
            "--packVersion",
            settings.Version!,
            "--packDir",
            paths.PublishDirectory(runtime.RuntimeIdentifier),
            "--mainExe",
            runtime.MainExecutable,
            "--runtime",
            runtime.RuntimeIdentifier,
            "--channel",
            GetChannel(runtime, settings),
            "--outputDir",
            string.IsNullOrWhiteSpace(artifactDirectory)
                ? paths.ArtifactDirectory(runtime.RuntimeIdentifier)
                : artifactDirectory,
        ];

    private string GetChannel() => GetChannel(_runtime, _settings);

    private static string GetChannel(DesktopRuntime runtime, DesktopCommandSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.Channel))
        {
            return settings.Channel;
        }

        return IsDevRelease(settings)
            ? $"{runtime.RuntimeIdentifier}-dev"
            : $"{runtime.RuntimeIdentifier}-stable";
    }

    private static bool IsDevRelease(DesktopCommandSettings settings) => !string.IsNullOrWhiteSpace(settings.InformationalVersion) && settings.InformationalVersion.Contains("dev", StringComparison.OrdinalIgnoreCase);
}

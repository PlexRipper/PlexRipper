using Microsoft.Extensions.Logging;

namespace Reaparr.Build;

internal sealed class DesktopBuildWorkflow
{
    private readonly BuildPaths _paths;
    private readonly DesktopRuntime _runtime;
    private readonly DesktopPublishWorkflow _publishWorkflow;
    private readonly DesktopPackageWorkflow _packageWorkflow;
    private readonly DesktopLaunchWorkflow _launchWorkflow;
    private readonly IDesktopCommandRunner _commandRunner;
    private readonly DesktopCommandSettings _settings;
    private readonly FileSystemTasks _fileSystemTasks;
    private readonly System.IO.Abstractions.IFileSystem _fileSystem;
    private readonly ILogger<DesktopBuildWorkflow> _logger;
    public DesktopBuildWorkflow(BuildPaths paths,
        DesktopRuntime runtime,
        DesktopPublishWorkflow publishWorkflow,
        DesktopPackageWorkflow packageWorkflow,
        DesktopLaunchWorkflow launchWorkflow,
        IDesktopCommandRunner commandRunner,
        DesktopCommandSettings settings,
        FileSystemTasks fileSystemTasks,
        System.IO.Abstractions.IFileSystem fileSystem,
        ILogger<DesktopBuildWorkflow> logger)
    {
        _paths = paths;
        _runtime = runtime;
        _publishWorkflow = publishWorkflow;
        _packageWorkflow = packageWorkflow;
        _launchWorkflow = launchWorkflow;
        _commandRunner = commandRunner;
        _settings = settings;
        _fileSystemTasks = fileSystemTasks;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public static DesktopBuildWorkflow Create(
        BuildPaths paths,
        DesktopCommandSettings settings,
        ILoggerFactory loggerFactory,
        System.IO.Abstractions.IFileSystem fileSystem
    ) =>
        Create(
            paths,
            settings,
            loggerFactory,
            fileSystem,
            new DesktopCommandRunner(
                paths,
                settings,
                fileSystem,
                loggerFactory.CreateLogger<DesktopCommandRunner>()
            )
        );

    internal static DesktopBuildWorkflow Create(
        BuildPaths paths,
        DesktopCommandSettings settings,
        ILoggerFactory loggerFactory,
        System.IO.Abstractions.IFileSystem fileSystem,
        IDesktopCommandRunner commandRunner
    )
    {
        var runtime = DesktopRuntimeCatalog.Get(settings.RuntimeIdentifier);
        var fileSystemTasks = new FileSystemTasks(fileSystem);
        var publishWorkflow = new DesktopPublishWorkflow(
            paths,
            runtime,
            settings,
            commandRunner,
            fileSystemTasks,
            fileSystem,
            loggerFactory.CreateLogger<DesktopPublishWorkflow>()
        );
        var packageWorkflow = new DesktopPackageWorkflow(
            paths,
            runtime,
            settings,
            commandRunner,
            fileSystemTasks,
            fileSystem,
            loggerFactory.CreateLogger<DesktopPackageWorkflow>()
        );
        var launchWorkflow = new DesktopLaunchWorkflow(
            paths,
            runtime,
            settings,
            commandRunner,
            packageWorkflow,
            fileSystem,
            loggerFactory.CreateLogger<DesktopLaunchWorkflow>()
        );

        return new DesktopBuildWorkflow(
            paths,
            runtime,
            publishWorkflow,
            packageWorkflow,
            launchWorkflow,
            commandRunner,
            settings,
            fileSystemTasks,
            fileSystem,
            loggerFactory.CreateLogger<DesktopBuildWorkflow>()
        );
    }

    public async Task<int> PublishAsync()
    {
        _logger.LogDebug("Starting publish workflow");
        await _publishWorkflow.PublishAsync();
        return 0;
    }

    public async Task<int> PackageAsync()
    {
        _logger.LogDebug("Starting package workflow");
        await ValidatePackagingPrerequisitesAsync();
        await _publishWorkflow.PublishAsync();
        await _packageWorkflow.PackageAsync();
        return 0;
    }

    public async Task<int> RunAsync()
    {
        _logger.LogInformation(
            "Starting run workflow for {RuntimeIdentifier} (SkipPackage={SkipPackage}, DryRun={DryRun})",
            _settings.RuntimeIdentifier,
            _settings.SkipPackage,
            _settings.DryRun
        );

        if (_settings.SkipPackage)
        {
            _logger.LogInformation("Skipping packaging step for {RuntimeIdentifier}; launching published output directly", _settings.RuntimeIdentifier);
            await _publishWorkflow.PublishAsync();
            EnsureWindowsArtifactExport();
        }
        else
        {
            await ValidatePackagingPrerequisitesAsync();

            _logger.LogInformation("Publishing desktop build for {RuntimeIdentifier}", _settings.RuntimeIdentifier);
            await _publishWorkflow.PublishAsync();

            _logger.LogInformation("Packaging desktop build for {RuntimeIdentifier}", _settings.RuntimeIdentifier);
            await _packageWorkflow.PackageAsync();
        }

        if (_settings.DryRun)
        {
            _logger.LogInformation("Dry-run enabled; skipping launch step for {RuntimeIdentifier}", _settings.RuntimeIdentifier);
            return 0;
        }

        _logger.LogInformation("Launching desktop build for {RuntimeIdentifier}", _settings.RuntimeIdentifier);
        return await _launchWorkflow.LaunchAsync();
    }

    private async Task ValidatePackagingPrerequisitesAsync()
    {
        if (_settings.DryRun)
        {
            return;
        }

        await _commandRunner.RequireCommandAsync("vpk");
    }

    private void EnsureWindowsArtifactExport()
    {
        if (!_runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var artifactDirectory = _packageWorkflow.GetArtifactDirectory();
        _fileSystem.Directory.CreateDirectory(artifactDirectory);

        var publishedExecutable = _fileSystem.Path.Combine(_paths.PublishDirectory(_runtime.RuntimeIdentifier), _runtime.MainExecutable);
        if (_fileSystem.File.Exists(publishedExecutable))
        {
            var targetExecutable = _fileSystem.Path.Combine(artifactDirectory, _runtime.MainExecutable);
            _fileSystem.File.Copy(publishedExecutable, targetExecutable, overwrite: true);
            _logger.LogInformation("Exported Windows executable artifact to {ArtifactPath}", targetExecutable);
        }

        var publishedRoot = _paths.PublishDirectory(_runtime.RuntimeIdentifier);
        var targetRoot = _fileSystem.Path.Combine(artifactDirectory, "publish");
        _fileSystemTasks.ClearDirectory(targetRoot);
        _fileSystemTasks.CopyDirectory(publishedRoot, targetRoot);

        _logger.LogInformation("Exported Windows publish directory to {ArtifactPublishDirectory}", targetRoot);
    }
}

using Microsoft.Extensions.Logging;

namespace Reaparr.Build;

internal sealed class DesktopBuildWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopPublishWorkflow publishWorkflow,
    DesktopPackageWorkflow packageWorkflow,
    DesktopLaunchWorkflow launchWorkflow,
    DesktopCommandSettings settings,
    FileSystemTasks fileSystemTasks,
    System.IO.Abstractions.IFileSystem fileSystem,
    ILogger<DesktopBuildWorkflow> logger
)
{
    public static DesktopBuildWorkflow Create(
        BuildPaths paths,
        DesktopCommandSettings settings,
        ILoggerFactory loggerFactory,
        System.IO.Abstractions.IFileSystem fileSystem
    )
    {
        var runtime = DesktopRuntimeCatalog.Get(settings.RuntimeIdentifier);
        var commandRunner = new DesktopCommandRunner(
            paths,
            settings,
            fileSystem,
            loggerFactory.CreateLogger<DesktopCommandRunner>()
        );
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
            settings,
            fileSystemTasks,
            fileSystem,
            loggerFactory.CreateLogger<DesktopBuildWorkflow>()
        );
    }

    public async Task<int> PublishAsync()
    {
        logger.LogDebug("Starting publish workflow");
        await publishWorkflow.PublishAsync();
        return 0;
    }

    public async Task<int> PackageAsync()
    {
        logger.LogDebug("Starting package workflow");
        await publishWorkflow.PublishAsync();
        await packageWorkflow.PackageAsync();
        return 0;
    }

    public async Task<int> RunAsync()
    {
        ValidateLaunchMode();

        logger.LogInformation(
            "Starting run workflow for {RuntimeIdentifier} (SkipPackage={SkipPackage}, DryRun={DryRun})",
            settings.RuntimeIdentifier,
            settings.SkipPackage,
            settings.DryRun
        );

        if (settings.SkipPackage)
        {
            logger.LogInformation("Skipping packaging step for {RuntimeIdentifier}; launching published output directly", settings.RuntimeIdentifier);
            await publishWorkflow.PublishAsync();
            EnsureWindowsArtifactExport();
        }
        else
        {
            logger.LogInformation("Publishing desktop build for {RuntimeIdentifier}", settings.RuntimeIdentifier);
            await publishWorkflow.PublishAsync();

            logger.LogInformation("Packaging desktop build for {RuntimeIdentifier}", settings.RuntimeIdentifier);
            await packageWorkflow.PackageAsync();
        }

        if (settings.DryRun)
        {
            logger.LogInformation("Dry-run enabled; skipping launch step for {RuntimeIdentifier}", settings.RuntimeIdentifier);
            return 0;
        }

        logger.LogInformation("Launching desktop build for {RuntimeIdentifier}", settings.RuntimeIdentifier);
        return await launchWorkflow.LaunchAsync();
    }

    private void ValidateLaunchMode()
    {
        if (!runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase) || settings.SkipPackage)
        {
            return;
        }

        if (
            string.Equals(settings.LaunchMode, "published", StringComparison.OrdinalIgnoreCase)
            || string.Equals(settings.LaunchMode, "packaged", StringComparison.OrdinalIgnoreCase)
        )
        {
            return;
        }

        throw new ArgumentException(
            $"Unsupported launch mode '{settings.LaunchMode}'. Supported values are 'published' and 'packaged'.",
            nameof(settings.LaunchMode)
        );
    }

    private void EnsureWindowsArtifactExport()
    {
        if (!runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var artifactDirectory = packageWorkflow.GetArtifactDirectory();
        fileSystem.Directory.CreateDirectory(artifactDirectory);

        var publishedExecutable = fileSystem.Path.Combine(paths.PublishDirectory(runtime.RuntimeIdentifier), runtime.MainExecutable);
        if (fileSystem.File.Exists(publishedExecutable))
        {
            var targetExecutable = fileSystem.Path.Combine(artifactDirectory, runtime.MainExecutable);
            fileSystem.File.Copy(publishedExecutable, targetExecutable, overwrite: true);
            logger.LogInformation("Exported Windows executable artifact to {ArtifactPath}", targetExecutable);
        }

        var publishedRoot = paths.PublishDirectory(runtime.RuntimeIdentifier);
        var targetRoot = fileSystem.Path.Combine(artifactDirectory, "publish");
        fileSystemTasks.ClearDirectory(targetRoot);
        fileSystemTasks.CopyDirectory(publishedRoot, targetRoot);

        logger.LogInformation("Exported Windows publish directory to {ArtifactPublishDirectory}", targetRoot);
    }
}

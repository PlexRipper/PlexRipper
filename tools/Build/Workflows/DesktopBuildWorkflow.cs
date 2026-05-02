using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopBuildWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopPublishWorkflow publishWorkflow,
    DesktopPackageWorkflow packageWorkflow,
    DesktopLaunchWorkflow launchWorkflow,
    DesktopCommandSettings settings,
    ILogger<DesktopBuildWorkflow> logger
)
{
    public static DesktopBuildWorkflow Create(
        BuildPaths paths,
        DesktopCommandSettings settings,
        ILoggerFactory loggerFactory
    )
    {
        var runtime = DesktopRuntimeCatalog.Get(settings.RuntimeIdentifier);
        var commandRunner = new DesktopCommandRunner(
            paths,
            settings,
            loggerFactory.CreateLogger<DesktopCommandRunner>()
        );
        var publishWorkflow = new DesktopPublishWorkflow(
            paths,
            runtime,
            settings,
            commandRunner,
            loggerFactory.CreateLogger<DesktopPublishWorkflow>()
        );
        var packageWorkflow = new DesktopPackageWorkflow(
            paths,
            runtime,
            settings,
            commandRunner,
            loggerFactory.CreateLogger<DesktopPackageWorkflow>()
        );
        var launchWorkflow = new DesktopLaunchWorkflow(
            paths,
            runtime,
            settings,
            commandRunner,
            packageWorkflow,
            loggerFactory.CreateLogger<DesktopLaunchWorkflow>()
        );

        return new DesktopBuildWorkflow(
            paths,
            runtime,
            publishWorkflow,
            packageWorkflow,
            launchWorkflow,
            settings,
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

    private void EnsureWindowsArtifactExport()
    {
        if (!runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var artifactDirectory = packageWorkflow.GetArtifactDirectory();
        Directory.CreateDirectory(artifactDirectory);

        var publishedExecutable = Path.Combine(paths.PublishDirectory(runtime.RuntimeIdentifier), runtime.MainExecutable);
        if (File.Exists(publishedExecutable))
        {
            var targetExecutable = Path.Combine(artifactDirectory, runtime.MainExecutable);
            File.Copy(publishedExecutable, targetExecutable, overwrite: true);
            logger.LogInformation("Exported Windows executable artifact to {ArtifactPath}", targetExecutable);
        }

        var publishedRoot = new DirectoryInfo(paths.PublishDirectory(runtime.RuntimeIdentifier));
        var targetRoot = new DirectoryInfo(Path.Combine(artifactDirectory, "publish"));
        FileSystemTasks.ClearDirectory(targetRoot);
        FileSystemTasks.CopyDirectory(publishedRoot, targetRoot);

        logger.LogInformation("Exported Windows publish directory to {ArtifactPublishDirectory}", targetRoot.FullName);
    }
}

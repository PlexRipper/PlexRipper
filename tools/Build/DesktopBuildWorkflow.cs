using Microsoft.Extensions.Logging;

namespace Reaparr.Build;

internal sealed class DesktopBuildWorkflow(
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
        logger.LogDebug("Starting run workflow");

        if (settings.SkipPackage)
        {
            await publishWorkflow.PublishAsync();
        }
        else
        {
            await publishWorkflow.PublishAsync();
            await packageWorkflow.PackageAsync();
        }

        if (settings.DryRun)
        {
            return 0;
        }

        return await launchWorkflow.LaunchAsync();
    }
}

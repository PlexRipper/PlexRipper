using Microsoft.Extensions.Logging;

namespace Reaparr.Build;

internal sealed class DesktopLaunchWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    DesktopCommandRunner commandRunner,
    DesktopPackageWorkflow packageWorkflow,
    ILogger<DesktopLaunchWorkflow> logger
)
{
    public async Task<int> LaunchAsync()
    {
        if (runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase) && !settings.SkipPackage)
        {
            var appImagePath = FindLinuxAppImage();
            logger.LogInformation(
                "Validated packaged Linux AppImage for {RuntimeIdentifier} at {AppImagePath}; launching published executable for an attached desktop run session",
                runtime.RuntimeIdentifier,
                appImagePath
            );
            await commandRunner.RunCommandAsync("chmod", ["+x", appImagePath]);
        }

        var publishedExecutable = Path.Combine(
            paths.PublishDirectory(runtime.RuntimeIdentifier),
            runtime.MainExecutable
        );

        logger.LogInformation(
            "Resolved published executable for {RuntimeIdentifier} to {PublishedExecutable}",
            runtime.RuntimeIdentifier,
            publishedExecutable
        );

        if (!File.Exists(publishedExecutable))
        {
            throw new FileNotFoundException(
                $"Published executable for runtime '{runtime.RuntimeIdentifier}' was not found at '{publishedExecutable}'.",
                publishedExecutable
            );
        }

        if (
            runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase)
            && !OperatingSystem.IsWindows()
        )
        {
            if (await commandRunner.CommandExistsAsync("wine"))
            {
                logger.LogInformation(
                    "Launching Windows desktop build for {RuntimeIdentifier} with wine using {PublishedExecutable}",
                    runtime.RuntimeIdentifier,
                    publishedExecutable
                );

                var wineExitCode = await commandRunner.ExecuteCommandAsync("wine", [publishedExecutable]);
                logger.LogInformation(
                    "Wine launch for {RuntimeIdentifier} exited with code {ExitCode}",
                    runtime.RuntimeIdentifier,
                    wineExitCode
                );

                return wineExitCode;
            }

            logger.LogInformation(
                "Published {RuntimeIdentifier} build to {PublishedExecutable}",
                runtime.RuntimeIdentifier,
                publishedExecutable
            );
            logger.LogInformation(
                "Packaged {RuntimeIdentifier} artifacts to {ArtifactDirectory}",
                runtime.RuntimeIdentifier,
                packageWorkflow.GetArtifactDirectory()
            );
            logger.LogWarning("Wine is not available on this host, so the Windows build was not launched.");
            return 0;
        }

        logger.LogInformation(
            "Launching published executable for {RuntimeIdentifier} directly from {PublishedExecutable}",
            runtime.RuntimeIdentifier,
            publishedExecutable
        );

        var exitCode = await commandRunner.ExecuteCommandAsync(publishedExecutable, []);
        logger.LogInformation(
            "Published executable for {RuntimeIdentifier} exited with code {ExitCode}",
            runtime.RuntimeIdentifier,
            exitCode
        );

        return exitCode;
    }

    private string FindLinuxAppImage()
    {
        var artifactDirectory = new DirectoryInfo(packageWorkflow.GetArtifactDirectory());
        logger.LogInformation(
            "Searching for packaged Linux AppImage for {RuntimeIdentifier} in {ArtifactDirectory}",
            runtime.RuntimeIdentifier,
            artifactDirectory.FullName
        );

        if (!artifactDirectory.Exists)
        {
            throw new DirectoryNotFoundException(
                $"Artifact directory for runtime '{runtime.RuntimeIdentifier}' was not found at '{artifactDirectory.FullName}'."
            );
        }

        var appImage = artifactDirectory
            .GetFiles("*.AppImage", SearchOption.TopDirectoryOnly)
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (appImage is null)
        {
            throw new FileNotFoundException(
                $"No AppImage artifact was produced for runtime '{runtime.RuntimeIdentifier}' in '{artifactDirectory.FullName}'."
            );
        }

        logger.LogInformation(
            "Selected Linux AppImage for {RuntimeIdentifier}: {AppImagePath}",
            runtime.RuntimeIdentifier,
            appImage.FullName
        );

        return appImage.FullName;
    }
}

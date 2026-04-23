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
            await commandRunner.RunCommandAsync("chmod", ["+x", appImagePath]);
            return await commandRunner.ExecuteCommandAsync(appImagePath, []);
        }

        var publishedExecutable = Path.Combine(
            paths.PublishDirectory(runtime.RuntimeIdentifier),
            runtime.MainExecutable
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
                return await commandRunner.ExecuteCommandAsync("wine", [publishedExecutable]);
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

        return await commandRunner.ExecuteCommandAsync(publishedExecutable, []);
    }

    private string FindLinuxAppImage()
    {
        var artifactDirectory = new DirectoryInfo(packageWorkflow.GetArtifactDirectory());
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

        return appImage.FullName;
    }
}

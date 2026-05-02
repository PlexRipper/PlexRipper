using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopLaunchWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    DesktopCommandRunner commandRunner,
    DesktopPackageWorkflow packageWorkflow,
    IFileSystem fileSystem,
    ILogger<DesktopLaunchWorkflow> logger
)
{
    private const string LAUNCH_MODE_PACKAGED = "packaged";
    private const string LAUNCH_MODE_PUBLISHED = "published";

    public async Task<int> LaunchAsync()
    {
        if (
            runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase)
            && !settings.SkipPackage
            && string.Equals(settings.LaunchMode, LAUNCH_MODE_PACKAGED, StringComparison.OrdinalIgnoreCase)
        )
        {
            var appImagePath = FindLinuxAppImage();
            logger.LogInformation(
                "Launching packaged Linux AppImage for {RuntimeIdentifier} from {AppImagePath}",
                runtime.RuntimeIdentifier,
                appImagePath
            );

            await commandRunner.RunCommandAsync("chmod", ["+x", appImagePath]);

            var appImageExitCode = await commandRunner.ExecuteCommandAsync(appImagePath, []);
            logger.LogInformation(
                "Packaged Linux AppImage for {RuntimeIdentifier} exited with code {ExitCode}",
                runtime.RuntimeIdentifier,
                appImageExitCode
            );

            return appImageExitCode;
        }

        if (
            runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase)
            && !settings.SkipPackage
            && !string.Equals(settings.LaunchMode, LAUNCH_MODE_PUBLISHED, StringComparison.OrdinalIgnoreCase)
            && !string.Equals(settings.LaunchMode, LAUNCH_MODE_PACKAGED, StringComparison.OrdinalIgnoreCase)
        )
        {
            throw new ArgumentException(
                $"Unsupported launch mode '{settings.LaunchMode}'. Supported values are '{LAUNCH_MODE_PUBLISHED}' and '{LAUNCH_MODE_PACKAGED}'.",
                nameof(settings.LaunchMode)
            );
        }

        var publishedExecutable = fileSystem.Path.Combine(
            paths.PublishDirectory(runtime.RuntimeIdentifier),
            runtime.MainExecutable
        );

        logger.LogInformation(
            "Resolved published executable for {RuntimeIdentifier} to {PublishedExecutable} (launch mode: {LaunchMode})",
            runtime.RuntimeIdentifier,
            publishedExecutable,
            settings.LaunchMode
        );

        if (!fileSystem.File.Exists(publishedExecutable))
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
        var artifactDirectory = packageWorkflow.GetArtifactDirectory();
        logger.LogInformation(
            "Searching for packaged Linux AppImage for {RuntimeIdentifier} in {ArtifactDirectory}",
            runtime.RuntimeIdentifier,
            artifactDirectory
        );

        if (!fileSystem.Directory.Exists(artifactDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Artifact directory for runtime '{runtime.RuntimeIdentifier}' was not found at '{artifactDirectory}'."
            );
        }

        var appImage = fileSystem.Directory
            .EnumerateFiles(artifactDirectory, "*.AppImage", SearchOption.TopDirectoryOnly)
            .Select(file => fileSystem.FileInfo.New(file))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();

        if (appImage is null)
        {
            throw new FileNotFoundException(
                $"No AppImage artifact was produced for runtime '{runtime.RuntimeIdentifier}' in '{artifactDirectory}'."
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

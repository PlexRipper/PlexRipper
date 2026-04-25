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

            var movedPackages = ClearPendingVelopackPackages(GetVelopackPackageRoot());
            if (movedPackages.Count > 0)
            {
                logger.LogWarning(
                    "Moved {MovedPackageCount} pending Velopack package(s) to prevent automatic apply on launch: {MovedPackages}",
                    movedPackages.Count,
                    string.Join(", ", movedPackages)
                );
            }

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

        var publishedExecutable = Path.Combine(
            paths.PublishDirectory(runtime.RuntimeIdentifier),
            runtime.MainExecutable
        );

        logger.LogInformation(
            "Resolved published executable for {RuntimeIdentifier} to {PublishedExecutable} (launch mode: {LaunchMode})",
            runtime.RuntimeIdentifier,
            publishedExecutable,
            settings.LaunchMode
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

    private static string GetVelopackPackageRoot() =>
        Path.Combine(Path.GetTempPath(), "velopack", "Reaparr");

    internal static List<string> ClearPendingVelopackPackages(string velopackPackageRoot)
    {
        var packagesDirectory = Path.Combine(velopackPackageRoot, "packages");
        if (!Directory.Exists(packagesDirectory))
            return [];

        var pendingPackages = Directory
            .GetFiles(packagesDirectory, "*.nupkg", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.Ordinal)
            .ToList();
        if (pendingPackages.Count == 0)
            return [];

        var holdDirectory = Path.Combine(velopackPackageRoot, "packages-hold");
        Directory.CreateDirectory(holdDirectory);

        var movedPackages = new List<string>(pendingPackages.Count);
        foreach (var pendingPackage in pendingPackages)
        {
            var destinationPath = Path.Combine(holdDirectory, Path.GetFileName(pendingPackage));
            if (File.Exists(destinationPath))
                File.Delete(destinationPath);

            File.Move(pendingPackage, destinationPath);
            movedPackages.Add(destinationPath);
        }

        return movedPackages;
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

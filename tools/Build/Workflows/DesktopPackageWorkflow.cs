using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopPackageWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    DesktopCommandRunner commandRunner,
    FileSystemTasks fileSystemTasks,
    IFileSystem fileSystem,
    ILogger<DesktopPackageWorkflow> logger
)
{
    private const string PACKAGE_ID = "Reaparr";
    private const string PACKAGE_TITLE = "Reaparr";

    public async Task PackageAsync()
    {
        if (!settings.DryRun)
        {
            await commandRunner.RequireCommandAsync("vpk");

            if (!settings.PreserveExistingArtifacts)
            {
                fileSystemTasks.ClearArtifactDirectory(paths.RootDirectory, GetArtifactDirectory());
            }
            else
            {
                fileSystem.Directory.CreateDirectory(GetArtifactDirectory());
            }
        }

        await commandRunner.RunCommandAsync("vpk", CreatePackArguments(paths, runtime, settings, GetArtifactDirectory()));

        logger.LogInformation(
            "Packaged {RuntimeIdentifier} desktop artifacts for Velopack channel {Channel} to {ArtifactDirectory}",
            runtime.RuntimeIdentifier,
            GetChannel(),
            GetArtifactDirectory()
        );
    }

    public string GetArtifactDirectory() =>
        string.IsNullOrWhiteSpace(settings.ArtifactDirectory)
            ? paths.ArtifactDirectory(runtime.RuntimeIdentifier)
            : fileSystem.Path.GetFullPath(settings.ArtifactDirectory, paths.RootDirectory);

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

    private string GetChannel() => GetChannel(runtime, settings);

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

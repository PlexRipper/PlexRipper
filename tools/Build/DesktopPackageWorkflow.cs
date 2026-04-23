using Microsoft.Extensions.Logging;
using Reaparr.Environment;

namespace Reaparr.Build;

internal sealed class DesktopPackageWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    DesktopCommandRunner commandRunner,
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
                FileSystemTasks.ClearDirectory(new DirectoryInfo(GetArtifactDirectory()));
            }
            else
            {
                Directory.CreateDirectory(GetArtifactDirectory());
            }
        }

        await commandRunner.RunCommandAsync(
            "vpk",
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
                "--channel",
                GetChannel(),
                "--outputDir",
                GetArtifactDirectory(),
            ]
        );

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
            : Path.GetFullPath(settings.ArtifactDirectory, paths.RootDirectory.FullName);

    private string GetChannel() =>
        string.IsNullOrWhiteSpace(settings.Channel)
            ? (EnvironmentExtensions.IsDevRelease() ? "dev" : "stable")
            : settings.Channel;
}

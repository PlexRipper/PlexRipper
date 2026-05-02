using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopPublishWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    IDesktopCommandRunner commandRunner,
    FileSystemTasks fileSystemTasks,
    IFileSystem fileSystem,
    ILogger<DesktopPublishWorkflow> logger
)
{
    public async Task PublishAsync()
    {
        ValidateRequiredBuildMetadata();

        if (!settings.DryRun)
        {
            await commandRunner.RequireCommandAsync("dotnet");
        }

        await GenerateFrontendAsync();
        await RestoreAsync();
        await PublishAppHostAsync();
        CopyFrontendOutput();

        logger.LogInformation(
            "Published {RuntimeIdentifier} desktop build to {PublishDirectory}",
            runtime.RuntimeIdentifier,
            paths.PublishDirectory(runtime.RuntimeIdentifier)
        );
    }

    private void ValidateRequiredBuildMetadata()
    {
        if (string.IsNullOrWhiteSpace(settings.Version))
        {
            throw new ArgumentException(
                "A build version is required. Pass --version <VERSION>.",
                nameof(settings.Version)
            );
        }

        if (string.IsNullOrWhiteSpace(settings.InformationalVersion))
        {
            throw new ArgumentException(
                "An informational version is required. Pass --informational-version <VERSION>.",
                nameof(settings.InformationalVersion)
            );
        }
    }

    private async Task GenerateFrontendAsync()
    {
        if (settings.SkipFrontend)
        {
            return;
        }

        var sourceDirectory = GetFrontendPublicDirectory();
        if (fileSystem.Directory.Exists(sourceDirectory))
        {
            logger.LogInformation("Reusing existing frontend output from {FrontendPublicDirectory}", sourceDirectory);
            return;
        }

        if (!settings.DryRun)
        {
            await commandRunner.RequireCommandAsync("bun");
        }

        if (ShouldInstallFrontendDependencies(paths.ClientAppDirectory))
        {
            logger.LogInformation("Installing frontend dependencies in {ClientAppDirectory}", paths.ClientAppDirectory);
            await commandRunner.RunCommandAsync("bun", ["install", "--frozen-lockfile"], paths.ClientAppDirectory);
        }

        await commandRunner.RunCommandAsync("bun", ["run", "generate", "--fail-on-error"], paths.ClientAppDirectory);
    }

    private async Task RestoreAsync()
    {
        if (settings.SkipRestore)
        {
            return;
        }

        await commandRunner.RunCommandAsync(
            "dotnet",
            ["restore", paths.AppHostProject, "--runtime", runtime.RuntimeIdentifier]
        );
    }

    private async Task PublishAppHostAsync()
    {
        var publishArgs = new List<string>
        {
            "publish",
            paths.AppHostProject,
            $"-p:PublishProfile={runtime.PublishProfile}",
            $"-p:Version={settings.Version}",
            $"-p:InformationalVersion={settings.InformationalVersion}",
            "-p:CSharpier_Bypass=true",
            "--no-restore",
        };

        await commandRunner.RunCommandAsync("dotnet", publishArgs);
    }

    private void CopyFrontendOutput()
    {
        if (settings.DryRun)
        {
            return;
        }

        var sourceDirectory = GetFrontendPublicDirectory();
        if (!fileSystem.Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Generated frontend output directory was not found at '{sourceDirectory}'."
            );
        }

        var wwwrootDirectory = fileSystem.Path.Combine(
            paths.PublishDirectory(runtime.RuntimeIdentifier),
            "wwwroot"
        );
        fileSystemTasks.ClearArtifactDirectory(paths.RootDirectory, wwwrootDirectory);
        fileSystemTasks.CopyDirectory(sourceDirectory, wwwrootDirectory);
    }

    private string GetFrontendPublicDirectory() =>
        string.IsNullOrWhiteSpace(settings.FrontendPublicDirectory)
            ? paths.FrontendPublicDirectory
            : fileSystem.Path.GetFullPath(settings.FrontendPublicDirectory, paths.RootDirectory);

    private bool ShouldInstallFrontendDependencies(string clientAppDirectory)
    {
        var bunLockPath = fileSystem.Path.Combine(clientAppDirectory, "bun.lock");
        var nodeModulesPath = fileSystem.Path.Combine(clientAppDirectory, "node_modules");

        return fileSystem.File.Exists(bunLockPath) && !fileSystem.Directory.Exists(nodeModulesPath);
    }
}

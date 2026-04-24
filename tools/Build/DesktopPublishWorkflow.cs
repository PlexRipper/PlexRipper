using Microsoft.Extensions.Logging;

namespace Reaparr.Build;

internal sealed class DesktopPublishWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    DesktopCommandRunner commandRunner,
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
            "--no-restore",
        };

        if (
            OperatingSystem.IsWindows()
            || runtime.RuntimeIdentifier.StartsWith("win-", StringComparison.OrdinalIgnoreCase)
        )
        {
            publishArgs.Insert(publishArgs.Count - 1, "-p:CSharpier_Bypass=true");
        }

        await commandRunner.RunCommandAsync("dotnet", publishArgs);
    }

    private void CopyFrontendOutput()
    {
        if (settings.DryRun)
        {
            return;
        }

        var sourceDirectory = new DirectoryInfo(GetFrontendPublicDirectory());
        if (!sourceDirectory.Exists)
        {
            throw new DirectoryNotFoundException(
                $"Generated frontend output directory was not found at '{sourceDirectory.FullName}'."
            );
        }

        var wwwrootDirectory = new DirectoryInfo(
            Path.Combine(paths.PublishDirectory(runtime.RuntimeIdentifier), "wwwroot")
        );
        wwwrootDirectory.Create();
        FileSystemTasks.CopyDirectory(sourceDirectory, wwwrootDirectory);
    }

    private string GetFrontendPublicDirectory() =>
        string.IsNullOrWhiteSpace(settings.FrontendPublicDirectory)
            ? paths.FrontendPublicDirectory
            : Path.GetFullPath(settings.FrontendPublicDirectory, paths.RootDirectory.FullName);

    private static bool ShouldInstallFrontendDependencies(string clientAppDirectory)
    {
        var bunLockPath = Path.Combine(clientAppDirectory, "bun.lock");
        var nodeModulesPath = Path.Combine(clientAppDirectory, "node_modules");

        return File.Exists(bunLockPath) && !Directory.Exists(nodeModulesPath);
    }
}

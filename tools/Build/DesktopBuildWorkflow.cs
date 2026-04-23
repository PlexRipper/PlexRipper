using CliWrap;
using CliWrap.Buffered;
using CliWrap.EventStream;
using CliWrap.Exceptions;
using Microsoft.Extensions.Logging;
using Reaparr.Environment;

namespace Reaparr.Build;

internal sealed class DesktopBuildWorkflow(
    BuildPaths paths,
    DesktopRuntime runtime,
    DesktopCommandSettings settings,
    ILogger<DesktopBuildWorkflow> logger
)
{
    private const string PackageId = "Reaparr";
    private const string PackageTitle = "Reaparr";

    public static DesktopBuildWorkflow Create(
        BuildPaths paths,
        DesktopCommandSettings settings,
        ILogger<DesktopBuildWorkflow> logger
    )
    {
        return new DesktopBuildWorkflow(paths, DesktopRuntimeCatalog.Get(settings.RuntimeIdentifier), settings, logger);
    }

    public async Task<int> PublishAsync()
    {
        if (!settings.DryRun)
        {
            await RequireCommandAsync("dotnet");
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

        return 0;
    }

    public async Task<int> PackageAsync()
    {
        await PublishAsync();

        if (!settings.DryRun)
        {
            await RequireCommandAsync("vpk");
            FileSystemTasks.ClearDirectory(new DirectoryInfo(paths.ArtifactDirectory(runtime.RuntimeIdentifier)));
        }

        var channel = settings.Channel ?? (EnvironmentExtensions.IsDevRelease() ? "dev" : "stable");

        await RunCommandAsync(
            "vpk",
            [
                "pack",
                "--packId",
                PackageId,
                "--packTitle",
                PackageTitle,
                "--packVersion",
                settings.InformationalVersion ?? settings.Version ?? "0.0.1",
                "--packDir",
                paths.PublishDirectory(runtime.RuntimeIdentifier),
                "--mainExe",
                runtime.MainExecutable,
                "--channel",
                channel,
                "--outputDir",
                paths.ArtifactDirectory(runtime.RuntimeIdentifier),
            ]
        );

        logger.LogInformation(
            "Packaged {RuntimeIdentifier} desktop artifacts for Velopack channel {Channel} to {ArtifactDirectory}",
            runtime.RuntimeIdentifier,
            channel,
            paths.ArtifactDirectory(runtime.RuntimeIdentifier)
        );

        return 0;
    }

    public async Task<int> RunAsync()
    {
        if (settings.SkipPackage)
        {
            await PublishAsync();
        }
        else
        {
            await PackageAsync();
        }

        if (settings.DryRun)
        {
            return 0;
        }

        return await LaunchAsync();
    }

    private async Task GenerateFrontendAsync()
    {
        if (settings.SkipFrontend)
        {
            return;
        }

        if (!settings.DryRun)
        {
            await RequireCommandAsync("bun");
        }

        await RunCommandAsync("bun", ["run", "generate", "--fail-on-error"], paths.ClientAppDirectory);
    }

    private async Task RestoreAsync()
    {
        if (settings.SkipRestore)
        {
            return;
        }

        await RunCommandAsync("dotnet", ["restore", paths.AppHostProject, "--runtime", runtime.RuntimeIdentifier]);
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

        await RunCommandAsync("dotnet", publishArgs);
    }

    private async Task<int> LaunchAsync()
    {
        if (runtime.RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase) && !settings.SkipPackage)
        {
            var appImagePath = FindLinuxAppImage();
            await RunCommandAsync("chmod", ["+x", appImagePath]);
            return await ExecuteCommandAsync(appImagePath, []);
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
            if (await CommandExistsAsync("wine"))
            {
                return await ExecuteCommandAsync("wine", [publishedExecutable]);
            }

            logger.LogInformation(
                "Published {RuntimeIdentifier} build to {PublishedExecutable}",
                runtime.RuntimeIdentifier,
                publishedExecutable
            );
            logger.LogInformation(
                "Packaged {RuntimeIdentifier} artifacts to {ArtifactDirectory}",
                runtime.RuntimeIdentifier,
                paths.ArtifactDirectory(runtime.RuntimeIdentifier)
            );
            logger.LogWarning("Wine is not available on this host, so the Windows build was not launched.");
            return 0;
        }

        return await ExecuteCommandAsync(publishedExecutable, []);
    }

    private string FindLinuxAppImage()
    {
        var artifactDirectory = new DirectoryInfo(paths.ArtifactDirectory(runtime.RuntimeIdentifier));
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

    private void CopyFrontendOutput()
    {
        if (settings.DryRun)
        {
            return;
        }

        var sourceDirectory = new DirectoryInfo(paths.FrontendPublicDirectory);
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

    private async Task RunCommandAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null
    )
    {
        LogCommand(fileName, arguments);
        if (settings.DryRun)
        {
            return;
        }

        try
        {
            var command = Cli.Wrap(fileName)
                .WithArguments(arguments)
                .WithWorkingDirectory(workingDirectory ?? paths.RootDirectory.FullName);

            await foreach (var commandEvent in command.ListenAsync())
            {
                switch (commandEvent)
                {
                    case StandardOutputCommandEvent stdOut when !string.IsNullOrWhiteSpace(stdOut.Text):
                        logger.LogDebug("{Output}", stdOut.Text);
                        break;
                    case StandardErrorCommandEvent stdErr when !string.IsNullOrWhiteSpace(stdErr.Text):
                        logger.LogWarning("{Output}", stdErr.Text);
                        break;
                }
            }
        }
        catch (CommandExecutionException ex)
        {
            throw new InvalidOperationException(
                $"Command '{fileName}' failed with exit code {ex.ExitCode}. Full command: {FormatCommand(fileName, arguments)}",
                ex
            );
        }
    }

    private async Task<int> ExecuteCommandAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null
    )
    {
        LogCommand(fileName, arguments);
        if (settings.DryRun)
        {
            return 0;
        }

        var result = await Cli.Wrap(fileName)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDirectory ?? paths.RootDirectory.FullName)
            .WithValidation(CommandResultValidation.None)
            .ExecuteAsync();

        return result.ExitCode;
    }

    private async Task<bool> CommandExistsAsync(string command)
    {
        var executable = OperatingSystem.IsWindows() ? "where" : "which";
        var result = await Cli.Wrap(executable)
            .WithArguments([command])
            .WithWorkingDirectory(paths.RootDirectory.FullName)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();

        return result.ExitCode is 0;
    }

    private async Task RequireCommandAsync(string command)
    {
        if (!await CommandExistsAsync(command))
        {
            throw new FileNotFoundException(
                $"Required command '{command}' was not found on PATH. Install it or adjust your environment before running desktop builds.",
                command
            );
        }
    }

    private void LogCommand(string fileName, IReadOnlyList<string> arguments)
    {
        logger.LogInformation("> {Command}", FormatCommand(fileName, arguments));
    }

    private static string FormatCommand(string fileName, IReadOnlyList<string> arguments) =>
        string.Join(' ', new[] { fileName }.Concat(arguments.Select(QuoteIfNeeded)));

    private static string QuoteIfNeeded(string value) =>
        value.Any(char.IsWhiteSpace) ? $"\"{value.Replace("\"", "\\\"")}\"" : value;
}

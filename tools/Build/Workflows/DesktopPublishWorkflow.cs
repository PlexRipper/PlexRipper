using Microsoft.Extensions.Logging;
using System.IO.Abstractions;

namespace Reaparr.Build;

internal sealed class DesktopPublishWorkflow
{
    private readonly BuildPaths _paths;
    private readonly DesktopRuntime _runtime;
    private readonly DesktopCommandSettings _settings;
    private readonly IDesktopCommandRunner _commandRunner;
    private readonly FileSystemTasks _fileSystemTasks;
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<DesktopPublishWorkflow> _logger;
    public DesktopPublishWorkflow(BuildPaths paths,
        DesktopRuntime runtime,
        DesktopCommandSettings settings,
        IDesktopCommandRunner commandRunner,
        FileSystemTasks fileSystemTasks,
        IFileSystem fileSystem,
        ILogger<DesktopPublishWorkflow> logger)
    {
        _paths = paths;
        _runtime = runtime;
        _settings = settings;
        _commandRunner = commandRunner;
        _fileSystemTasks = fileSystemTasks;
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task PublishAsync()
    {
        ValidateRequiredBuildMetadata();

        if (!_settings.DryRun)
        {
            await _commandRunner.RequireCommandAsync("dotnet");
        }

        await GenerateFrontendAsync();
        await RestoreAsync();
        await PublishAppHostAsync();
        CopyFrontendOutput();

        _logger.LogInformation(
            "Published {RuntimeIdentifier} desktop build to {PublishDirectory}",
            _runtime.RuntimeIdentifier,
            _paths.PublishDirectory(_runtime.RuntimeIdentifier)
        );
    }

    private void ValidateRequiredBuildMetadata()
    {
        if (string.IsNullOrWhiteSpace(_settings.Version))
        {
            throw new ArgumentException(
                "A build version is required. Pass --version <VERSION>.",
                nameof(_settings.Version)
            );
        }

        if (string.IsNullOrWhiteSpace(_settings.InformationalVersion))
        {
            throw new ArgumentException(
                "An informational version is required. Pass --informational-version <VERSION>.",
                nameof(_settings.InformationalVersion)
            );
        }
    }

    private async Task GenerateFrontendAsync()
    {
        if (_settings.SkipFrontend)
        {
            return;
        }

        var sourceDirectory = GetFrontendPublicDirectory();
        if (_fileSystem.Directory.Exists(sourceDirectory))
        {
            _logger.LogInformation("Reusing existing frontend output from {FrontendPublicDirectory}", sourceDirectory);
            return;
        }

        if (!_settings.DryRun)
        {
            await _commandRunner.RequireCommandAsync("bun");
        }

        if (ShouldInstallFrontendDependencies(_paths.ClientAppDirectory))
        {
            _logger.LogInformation("Installing frontend dependencies in {ClientAppDirectory}", _paths.ClientAppDirectory);
            await _commandRunner.RunCommandAsync("bun", ["install", "--frozen-lockfile"], _paths.ClientAppDirectory);
        }

        await _commandRunner.RunCommandAsync("bun", ["run", "generate", "--fail-on-error"], _paths.ClientAppDirectory);
    }

    private async Task RestoreAsync()
    {
        if (_settings.SkipRestore)
        {
            return;
        }

        await _commandRunner.RunCommandAsync(
            "dotnet",
            ["restore", _paths.AppHostProject, "--runtime", _runtime.RuntimeIdentifier]
        );
    }

    private async Task PublishAppHostAsync()
    {
        var publishArgs = new List<string>
        {
            "publish",
            _paths.AppHostProject,
            $"-p:PublishProfile={_runtime.PublishProfile}",
            $"-p:Version={_settings.Version}",
            $"-p:InformationalVersion={_settings.InformationalVersion}",
            "-p:CSharpier_Bypass=true",
            "--no-restore",
        };

        await _commandRunner.RunCommandAsync("dotnet", publishArgs);
    }

    private void CopyFrontendOutput()
    {
        if (_settings.DryRun)
        {
            return;
        }

        var sourceDirectory = GetFrontendPublicDirectory();
        if (!_fileSystem.Directory.Exists(sourceDirectory))
        {
            throw new DirectoryNotFoundException(
                $"Generated frontend output directory was not found at '{sourceDirectory}'."
            );
        }

        var wwwrootDirectory = _fileSystem.Path.Combine(
            _paths.PublishDirectory(_runtime.RuntimeIdentifier),
            "wwwroot"
        );
        _fileSystemTasks.ClearArtifactDirectory(_paths.RootDirectory, wwwrootDirectory);
        _fileSystemTasks.CopyDirectory(sourceDirectory, wwwrootDirectory);
    }

    private string GetFrontendPublicDirectory() =>
        string.IsNullOrWhiteSpace(_settings.FrontendPublicDirectory)
            ? _paths.FrontendPublicDirectory
            : _fileSystem.Path.GetFullPath(_settings.FrontendPublicDirectory, _paths.RootDirectory);

    private bool ShouldInstallFrontendDependencies(string clientAppDirectory)
    {
        var bunLockPath = _fileSystem.Path.Combine(clientAppDirectory, "bun.lock");
        var nodeModulesPath = _fileSystem.Path.Combine(clientAppDirectory, "node_modules");

        return _fileSystem.File.Exists(bunLockPath) && !_fileSystem.Directory.Exists(nodeModulesPath);
    }
}

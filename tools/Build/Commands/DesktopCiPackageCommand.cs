using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopCiPackageCommand : AsyncCommand<DesktopCommandSettings>
{
    private readonly BuildPaths _paths;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystem _fileSystem;
    public DesktopCiPackageCommand(BuildPaths paths, ILoggerFactory loggerFactory, IFileSystem fileSystem)
    {
        _paths = paths;
        _loggerFactory = loggerFactory;
        _fileSystem = fileSystem;
    }

    protected override Task<int> ExecuteAsync(
        CommandContext context,
        DesktopCommandSettings settings,
        CancellationToken cancellationToken
    )
    {
        var ciSettings = CreateCiSettings(settings);

        return DesktopBuildWorkflow.Create(_paths, ciSettings, _loggerFactory, _fileSystem).PackageAsync();
    }

    internal static DesktopCommandSettings CreateCiSettings(DesktopCommandSettings settings) =>
        new()
        {
            RuntimeIdentifier = settings.RuntimeIdentifier,
            Version = settings.Version,
            InformationalVersion = settings.InformationalVersion,
            Channel = settings.Channel,
            SkipFrontend = true,
            SkipRestore = false,
            SkipPackage = false,
            DryRun = settings.DryRun,
            FrontendPublicDirectory = settings.FrontendPublicDirectory,
            ArtifactDirectory = settings.ArtifactDirectory,
            PreserveExistingArtifacts = settings.PreserveExistingArtifacts,
            LaunchMode = settings.LaunchMode,
        };
}

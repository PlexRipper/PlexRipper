using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopCiPackageCommand(BuildPaths paths, ILoggerFactory loggerFactory)
    : AsyncCommand<DesktopCommandSettings>
{
    protected override Task<int> ExecuteAsync(
        CommandContext context,
        DesktopCommandSettings settings,
        CancellationToken cancellationToken
    )
    {
        var ciSettings = new DesktopCommandSettings
        {
            RuntimeIdentifier = settings.RuntimeIdentifier,
            Version = settings.Version,
            InformationalVersion = settings.InformationalVersion,
            Channel = settings.Channel,
            SkipFrontend = true,
            SkipRestore = true,
            SkipPackage = false,
            DryRun = settings.DryRun,
            FrontendPublicDirectory = settings.FrontendPublicDirectory,
            ArtifactDirectory = settings.ArtifactDirectory,
            PreserveExistingArtifacts = settings.PreserveExistingArtifacts,
        };

        return DesktopBuildWorkflow.Create(paths, ciSettings, loggerFactory).PackageAsync();
    }
}

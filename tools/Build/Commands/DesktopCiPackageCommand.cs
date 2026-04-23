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
        var ciSettings = CreateCiSettings(settings);

        return DesktopBuildWorkflow.Create(paths, ciSettings, loggerFactory).PackageAsync();
    }

    private static DesktopCommandSettings CreateCiSettings(DesktopCommandSettings settings) =>
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
        };
}

using Microsoft.Extensions.Logging;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopPackageCommand(BuildPaths paths, ILoggerFactory loggerFactory)
    : AsyncCommand<DesktopCommandSettings>
{
    protected override Task<int> ExecuteAsync(
        CommandContext context,
        DesktopCommandSettings settings,
        CancellationToken cancellationToken
    )
    {
        return DesktopBuildWorkflow.Create(paths, settings, loggerFactory).PackageAsync();
    }
}

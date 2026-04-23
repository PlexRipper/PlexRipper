using Microsoft.Extensions.Logging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopRunCommand(BuildPaths paths, ILogger<DesktopBuildWorkflow> logger)
    : AsyncCommand<DesktopCommandSettings>
{
    protected override Task<int> ExecuteAsync(
        CommandContext context,
        DesktopCommandSettings settings,
        CancellationToken cancellationToken
    )
    {
        return DesktopBuildWorkflow.Create(paths, settings, logger).RunAsync();
    }
}

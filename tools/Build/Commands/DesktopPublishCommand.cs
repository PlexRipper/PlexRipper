using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopPublishCommand(BuildPaths paths, ILoggerFactory loggerFactory, IFileSystem fileSystem)
    : AsyncCommand<DesktopCommandSettings>
{
    protected override Task<int> ExecuteAsync(
        CommandContext context,
        DesktopCommandSettings settings,
        CancellationToken cancellationToken
    )
    {
        return DesktopBuildWorkflow.Create(paths, settings, loggerFactory, fileSystem).PublishAsync();
    }
}

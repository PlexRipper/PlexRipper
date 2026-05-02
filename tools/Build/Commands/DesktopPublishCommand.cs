using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopPublishCommand : AsyncCommand<DesktopCommandSettings>
{
    private readonly BuildPaths _paths;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IFileSystem _fileSystem;
    public DesktopPublishCommand(BuildPaths paths, ILoggerFactory loggerFactory, IFileSystem fileSystem)
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
        return DesktopBuildWorkflow.Create(_paths, settings, _loggerFactory, _fileSystem).PublishAsync();
    }
}

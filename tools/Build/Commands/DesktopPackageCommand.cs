using Reaparr.Domain;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopPackageCommand(ICommandExecutor commandExecutor) : AsyncCommand<DesktopCommandSettings>
{
    protected override async Task<int> ExecuteAsync(
        CommandContext context,
        DesktopCommandSettings settings,
        CancellationToken cancellationToken
    )
    {
        var result = await commandExecutor.Send(new DesktopPackageBuildCommand(settings), cancellationToken);

        return result.IsFailed ? 1 : result.Value;
    }
}

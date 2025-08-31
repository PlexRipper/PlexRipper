using FastEndpoints;
using Reaparr.Logging;
using Serilog;

namespace Reaparr.Domain;

public class CommandExecutor : ICommandExecutor
{
    private readonly Serilog.ILogger _log;

    public CommandExecutor(ILogger log)
    {
        _log = log.ForContext<CommandExecutor>();
    }

    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        try
        {
            return await command.ExecuteAsync(ct);
        }
        catch (Exception e)
        {
            _log.ErrorResult(e);
            throw;
        }
    }
}

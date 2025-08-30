using FastEndpoints;
using Reaparr.Logging;

namespace Reaparr.Domain;

public class CommandExecutor : ICommandExecutor
{
    private readonly Logging.ILog<CommandExecutor> _log;

    public CommandExecutor(ILog<CommandExecutor> log)
    {
        _log = log;
    }

    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
    {
        try
        {
            return await command.ExecuteAsync(ct);
        }
        catch (Exception e)
        {
            _log.Error(e);
            throw;
        }
    }
}

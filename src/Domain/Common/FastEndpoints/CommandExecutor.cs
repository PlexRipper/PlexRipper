using FastEndpoints;
using Logging.Interface;

namespace PlexRipper.Domain;

public class CommandExecutor : ICommandExecutor
{
    private readonly ILog<CommandExecutor> _log;

    public CommandExecutor(ILog<CommandExecutor> log)
    {
        _log = log;
    }

    public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default)
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

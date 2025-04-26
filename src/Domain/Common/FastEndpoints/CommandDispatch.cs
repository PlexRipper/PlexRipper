using FastEndpoints;
using Logging.Interface;

namespace PlexRipper.Domain;

public class CommandDispatch : ICommandDispatch
{
    private readonly ILog<CommandDispatch> _log;

    public CommandDispatch(ILog<CommandDispatch> log)
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

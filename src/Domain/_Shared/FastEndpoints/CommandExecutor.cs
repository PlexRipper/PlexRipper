using FastEndpoints;

namespace Reaparr.Domain;

public class CommandExecutor : ICommandExecutor
{
    private readonly ILogger _log;

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
            _log.Here().ErrorResult(e);
            throw;
        }
    }
}

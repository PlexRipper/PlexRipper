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
        where TResult : ResultBase, new()
    {
        try
        {
            return await command.ExecuteAsync(ct);
        }
        catch (OperationCanceledException e) when (ct.IsCancellationRequested)
        {
            return CreateFailedResult<TResult>(e);
        }
        catch (Exception e)
        {
            _log.Here().ErrorResult(e);
            return CreateFailedResult<TResult>(e);
        }
    }

    private static TResult CreateFailedResult<TResult>(Exception e)
        where TResult : ResultBase, new()
    {
        var result = new TResult();
        result.Reasons.Add(new ExceptionalError(e));
        return result;
    }
}

namespace Reaparr.Domain;

public class CommandExecutor : ICommandExecutor
{
    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        where TResult : ResultBase, new()
    {
        var executionResult = await Result.Try(() => command.ExecuteAsync(ct));
        if (executionResult.IsSuccess)
            return executionResult.Value;

        var failedResult = new TResult();
        failedResult.Reasons.AddRange(executionResult.Errors);
        return failedResult;
    }
}

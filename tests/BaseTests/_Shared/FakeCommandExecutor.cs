namespace Reaparr.BaseTests;

/// <summary>
/// Test-only <see cref="ICommandExecutor"/> that intercepts selected commands before FastEndpoints dispatch.
/// This keeps integration-test fault injection working after FE 8.x stopped honoring Autofac
/// <see cref="ICommandHandler{TCommand,TResult}"/> overrides for command execution.
/// </summary>
public sealed class FakeCommandExecutor : ICommandExecutor
{
    private readonly Dictionary<Type, Func<object, CancellationToken, Task<object>>> _interceptors = [];

    public FakeCommandExecutor Intercept<TCommand, TResult>(Func<TCommand, CancellationToken, Task<TResult>> handler)
        where TCommand : ICommand<TResult>
        where TResult : ResultBase, new()
    {
        _interceptors[typeof(TCommand)] = async (command, ct) => await handler((TCommand)command, ct);
        return this;
    }

    public async Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        where TResult : ResultBase, new()
    {
        try
        {
            if (_interceptors.TryGetValue(command.GetType(), out var interceptor))
            {
                return (TResult)await interceptor(command, ct);
            }

            return await command.ExecuteAsync(ct);
        }
        catch (OperationCanceledException e) when (ct.IsCancellationRequested)
        {
            return CreateFailedResult<TResult>(e);
        }
        catch (Exception e)
        {
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

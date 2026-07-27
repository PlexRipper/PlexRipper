namespace Reaparr.Domain;

public interface ICommandExecutor
{
    /// <summary>
    /// Sends a command through the Reaparr command pipeline and returns its result, wrapping any exception in
    /// <typeparamref name="TResult" />. Exceptions are never thrown to callers; they are always wrapped in the returned
    /// result type.
    /// </summary>
    /// <param name="command">
    /// The command instance to execute with its matching FastEndpoints command handler.
    /// </param>
    /// <param name="ct">
    /// The cancellation token forwarded to the command pipeline and command handler.
    /// </param>
    /// <typeparam name="TResult">
    /// The FluentResults result type produced by the command, usually <see cref="Result" /> or
    /// <see cref="Result{TValue}" />.
    /// </typeparam>
    /// <returns>
    /// A task that resolves to the command result, containing success, validation or domain failures, and any wrapped
    /// exception errors.
    /// </returns>
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default)
        where TResult : ResultBase, new();
}
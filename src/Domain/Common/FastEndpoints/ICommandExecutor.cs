using FastEndpoints;

namespace PlexRipper.Domain;

public interface ICommandExecutor
{
    Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}

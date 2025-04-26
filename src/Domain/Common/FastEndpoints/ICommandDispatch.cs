using FastEndpoints;

namespace PlexRipper.Domain;

public interface ICommandDispatch
{
    Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}

using FastEndpoints;

namespace PlexRipper.Domain;

public interface ICommandExecutor
{
    Task<TResult> Send<TResult>(ICommand<TResult> command, CancellationToken ct = default);
}

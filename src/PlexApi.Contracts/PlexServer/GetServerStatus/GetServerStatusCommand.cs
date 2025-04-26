using Application.Contracts;
using FastEndpoints;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

/// <summary>
/// Attempts to connect to a server by the given <see cref="PlexServerConnection"/> and returns the <see cref="PlexServerStatus"/> based on the result.
/// </summary>
/// <returns>The Result is successful if the <see cref="PlexServerStatus"/> was created successfully, regardless of whether the connection was successful.</returns>
public record GetServerStatusCommand : ICommand<Result<PlexServerStatus>>
{
    /// <summary>
    ///  The <see cref="PlexServerConnection"/> to test for.
    /// </summary>
    public required int PlexServerConnectionId { get; init; }

    /// <summary>
    /// Progress action callback to notify of connection attempt progress.
    /// </summary>
    public required Action<PlexApiClientProgress>? ProgressAction { get; init; }
}

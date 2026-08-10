namespace Reaparr.Application.Contracts;

/// <summary>
/// Checks the current connections of every enabled Plex server.
/// </summary>
public record CheckAllPlexServerConnectionsCommand : ICommand<Result>;

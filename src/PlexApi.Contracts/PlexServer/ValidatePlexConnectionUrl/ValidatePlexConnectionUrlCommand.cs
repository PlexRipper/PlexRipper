using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Used to validate the connection URL to the Plex server.
/// </summary>
public record ValidatePlexConnectionUrlCommand(string PlexConnectionUrl) : ICommand<Result<ServerIdentityDTO>>;

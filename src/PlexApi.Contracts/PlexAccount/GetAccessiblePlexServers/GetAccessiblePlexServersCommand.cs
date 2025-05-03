using FastEndpoints;
using FluentResults;
using PlexRipper.Domain;

namespace PlexApi.Contracts;

/// <summary>
/// Retrieves the accessible <see cref="PlexServer">PlexServers</see> by this <see cref="PlexAccount"/> with the <see cref="PlexServerConnection">PlexServerConnections</see> from the Plex API.
/// </summary>
/// <param name="PlexAccountId"> The <see cref="PlexAccount"/> to use.</param>
/// <returns>Returns the list of <see cref="PlexServer">PlexServers</see> this <see cref="PlexAccount"/> has access too
/// and a separate list of tokens this account has to use to communicate with the <see cref="PlexServer"/></returns>
public record GetAccessiblePlexServersCommand(int PlexAccountId) : ICommand<Result<List<PlexServerAccessDTO>>>;

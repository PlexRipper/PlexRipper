using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

/// <summary>
/// Retrieves the latest accessible <see cref="PlexServer">PlexServers</see> for this <see cref="PlexAccount"/> from the PlexAPI and stores it in the Database.
/// </summary>
/// <param name="plexAccountId">The id of the <see cref="PlexAccount"/> to check.</param>
public record RefreshAccessiblePlexServersCommand(int plexAccountId) : IRequest<Result<List<PlexServer>>>;
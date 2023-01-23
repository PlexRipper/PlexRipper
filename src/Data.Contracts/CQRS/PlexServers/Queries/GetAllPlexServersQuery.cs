using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllPlexServersQuery : IRequest<Result<List<PlexServer>>>
{
    /// <summary>
    /// Retrieves all the  <see cref="PlexServer">PlexServers</see> currently in the database.
    /// </summary>
    /// <param name="includeConnections">Include <see cref="PlexServerConnection"/> relationship.</param>
    /// <param name="includeLibraries">Include <see cref="PlexLibrary"/> relationship.</param>
    public GetAllPlexServersQuery(bool includeConnections = false, bool includeLibraries = false)
    {
        IncludeConnections = includeConnections;
        IncludeLibraries = includeLibraries;
    }

    public bool IncludeConnections { get; }
    public bool IncludeLibraries { get; }
}
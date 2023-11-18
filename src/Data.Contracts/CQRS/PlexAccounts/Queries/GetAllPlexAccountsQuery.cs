using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

/// <summary>
/// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
/// </summary>
/// <param name="IncludePlexServers">Should include accessible <see cref="PlexLibrary">PlexLibraries</see> for each <see cref="PlexAccount">PlexAccount</see>.</param>
/// <param name="IncludePlexLibraries">Should include accessible <see cref="PlexServer">PlexServers</see> for each <see cref="PlexAccount">PlexAccount</see>.</param>
/// <param name="OnlyEnabled">Should only return enabled <see cref="PlexAccount">PlexAccounts</see>.</param>
/// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
public record GetAllPlexAccountsQuery
    (bool IncludePlexServers = false, bool IncludePlexLibraries = false, bool OnlyEnabled = false) : IRequest<Result<List<PlexAccount>>>;
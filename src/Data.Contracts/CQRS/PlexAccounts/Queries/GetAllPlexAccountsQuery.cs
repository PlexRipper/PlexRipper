using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class GetAllPlexAccountsQuery : IRequest<Result<List<PlexAccount>>>
{
    #region Constructors

    /// <summary>
    /// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
    /// </summary>
    /// <param name="includePlexLibraries">Should include accessible <see cref="PlexLibrary">PlexLibraries</see> for each <see cref="PlexAccount">PlexAccount</see>.</param>
    /// <param name="includePlexServers">Should include accessible <see cref="PlexServer">PlexServers</see> for each <see cref="PlexAccount">PlexAccount</see>.</param>
    /// <param name="onlyEnabled">Should only return enabled <see cref="PlexAccount">PlexAccounts</see>.</param>
    /// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
    public GetAllPlexAccountsQuery(bool includePlexServers = false, bool includePlexLibraries = false, bool onlyEnabled = false)
    {
        IncludePlexServers = includePlexServers;
        IncludePlexLibraries = includePlexLibraries;
        OnlyEnabled = onlyEnabled;
    }

    #endregion

    #region Properties

    public bool IncludePlexServers { get; }

    public bool IncludePlexLibraries { get; }

    public bool OnlyEnabled { get; }

    #endregion
}
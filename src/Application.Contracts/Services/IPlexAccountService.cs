using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexAccountService
{
    /// <summary>
    /// Creates an <see cref="PlexAccount"/> in the Database and performs an SetupAccountAsync().
    /// </summary>
    /// <param name="plexAccount">The unique account.</param>
    /// <returns>Returns the added account after setup.</returns>
    Task<Result<PlexAccount>> CreatePlexAccountAsync(PlexAccount plexAccount);

    /// <summary>
    /// Hard deletes the <see cref="PlexAccount"/> from the Database.
    /// </summary>
    /// <param name="plexAccountId"></param>
    /// <returns></returns>
    Task<Result> DeletePlexAccountAsync(int plexAccountId);

    Task<Result<PlexAccount>> UpdatePlexAccountAsync(PlexAccount plexAccount, bool inspectServers = false);

    /// <summary>
    /// Refreshes the <see cref="PlexServer"/> and <see cref="PlexLibrary"/> access of the <see cref="PlexAccount"/>.
    /// </summary>
    /// <param name="plexAccountId">Can be 0 to refresh all enabled PlexAccounts.</param>
    /// <returns>If successful.</returns>
    Task<Result> RefreshPlexAccount(int plexAccountId = 0);

    Task<Result<AuthPin>> Get2FAPin(string clientId);

    Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId);
}
using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexAccountService
{
    /// <summary>
    /// Refreshes the <see cref="PlexServer"/> and <see cref="PlexLibrary"/> access of the <see cref="PlexAccount"/>.
    /// </summary>
    /// <param name="plexAccountId">Can be 0 to refresh all enabled PlexAccounts.</param>
    /// <returns>If successful.</returns>
    Task<Result> RefreshPlexAccount(int plexAccountId = 0);

    Task<Result<AuthPin>> Get2FAPin(string clientId);

    Task<Result<AuthPin>> Check2FAPin(int pinId, string clientId);
}
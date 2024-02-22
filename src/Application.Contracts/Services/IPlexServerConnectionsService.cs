using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexServerConnectionsService
{
    #region Methods

    #region Public



    Task<Result> CheckAllConnectionsOfPlexServersByAccountIdAsync(int plexAccountId);

    #endregion

    #endregion
}
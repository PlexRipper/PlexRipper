using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexServerRepository
{
    Task<Result<List<PlexAccount>>> GetPlexAccountsWithAccess(int plexServerId, CancellationToken cancellationToken = default);
    Task<Result<PlexAccount>> ChoosePlexAccountToConnect(int plexServerId, CancellationToken cancellationToken = default);
}
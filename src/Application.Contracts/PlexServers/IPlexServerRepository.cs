using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IPlexServerRepository
{
    Task<Result<PlexAccount>> ChoosePlexAccountToConnect(int plexServerId, CancellationToken ct = default);
}
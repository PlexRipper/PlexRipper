using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IAddOrUpdatePlexAccountServersCommand
{
    Task<Result> ExecuteAsync(int plexAccountId, List<ServerAccessTokenDTO> serverAccessTokens, CancellationToken cancellationToken = default);
}
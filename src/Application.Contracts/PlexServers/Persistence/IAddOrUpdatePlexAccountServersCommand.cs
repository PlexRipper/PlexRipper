using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IAddOrUpdatePlexAccountServersCommand
{
    Task<Result> ExecuteAsync(PlexAccount plexAccount, List<ServerAccessTokenDTO> serverAccessTokens, CancellationToken cancellationToken);
}
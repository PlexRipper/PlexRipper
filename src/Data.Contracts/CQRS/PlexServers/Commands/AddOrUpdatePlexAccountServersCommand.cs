using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class AddOrUpdatePlexAccountServersCommand : IRequest<Result>
{
    public PlexAccount PlexAccount { get; }

    public List<ServerAccessTokenDTO> ServerAccessTokens { get; }

    public AddOrUpdatePlexAccountServersCommand(PlexAccount plexAccount, List<ServerAccessTokenDTO> serverAccessTokens)
    {
        PlexAccount = plexAccount;
        ServerAccessTokens = serverAccessTokens;
    }
}
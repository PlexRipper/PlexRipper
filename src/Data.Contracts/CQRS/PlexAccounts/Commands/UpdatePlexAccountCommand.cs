using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

public class UpdatePlexAccountCommand : IRequest<Result>
{
    public PlexAccount PlexAccount { get; }

    public UpdatePlexAccountCommand(PlexAccount plexAccount)
    {
        PlexAccount = plexAccount;
    }
}
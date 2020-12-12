using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexAccounts
{
    public class CreatePlexAccountCommand : IRequest<Result<int>>
    {
        public CreatePlexAccountCommand(PlexAccount plexAccount)
        {
            PlexAccount = plexAccount;
        }

        public PlexAccount PlexAccount { get; }
    }
}
using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace PlexRipper.Application.PlexAccounts
{
    public class UpdatePlexAccountCommand : IRequest<Result<bool>>
    {
        public PlexAccount PlexAccount { get; }

        public UpdatePlexAccountCommand(PlexAccount plexAccount)
        {
            PlexAccount = plexAccount;
        }
    }
}
using FluentResults;
using MediatR;

namespace PlexRipper.Application.PlexAccounts
{
    public class DeletePlexAccountCommand : IRequest<Result<bool>>
    {
        public int Id { get; }

        public DeletePlexAccountCommand(int Id)
        {
            this.Id = Id;
        }
    }
}
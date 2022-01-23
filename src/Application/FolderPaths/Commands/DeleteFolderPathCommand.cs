using FluentResults;
using MediatR;

namespace PlexRipper.Application
{
    public class DeleteFolderPathCommand : IRequest<Result>
    {
        public int Id { get; }

        public DeleteFolderPathCommand(int id)
        {
            Id = id;
        }
    }
}
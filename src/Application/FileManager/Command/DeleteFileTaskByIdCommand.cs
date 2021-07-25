using FluentResults;
using MediatR;

namespace PlexRipper.Application.FileManager.Command
{
    public class DeleteFileTaskByIdCommand: IRequest<Result>
    {
        public int Id { get; }

        public DeleteFileTaskByIdCommand(int id)
        {
            Id = id;
        }
    }


}
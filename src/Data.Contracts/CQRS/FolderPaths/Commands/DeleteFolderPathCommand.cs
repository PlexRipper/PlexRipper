using FluentResults;
using MediatR;

namespace Data.Contracts;

public class DeleteFolderPathCommand : IRequest<Result>
{
    public int Id { get; }

    public DeleteFolderPathCommand(int id)
    {
        Id = id;
    }
}
using FluentResults;
using MediatR;

namespace Data.Contracts;

public class DeleteFileTaskByIdCommand : IRequest<Result>
{
    public int Id { get; }

    public DeleteFileTaskByIdCommand(int id)
    {
        Id = id;
    }
}
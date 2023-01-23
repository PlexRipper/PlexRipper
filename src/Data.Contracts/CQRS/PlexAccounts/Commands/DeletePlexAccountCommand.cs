using FluentResults;
using MediatR;

namespace Data.Contracts;

public class DeletePlexAccountCommand : IRequest<Result>
{
    public int Id { get; }

    public DeletePlexAccountCommand(int Id)
    {
        this.Id = Id;
    }
}
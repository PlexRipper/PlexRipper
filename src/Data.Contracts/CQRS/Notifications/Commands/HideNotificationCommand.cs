using FluentResults;
using MediatR;

namespace Data.Contracts;

public class HideNotificationCommand : IRequest<Result>
{
    public int Id { get; }

    public HideNotificationCommand(int id)
    {
        Id = id;
    }
}
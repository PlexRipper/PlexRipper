using FluentResults;
using MediatR;

namespace PlexRipper.Application
{
    public class RemoveInaccessibleServersCommand : IRequest<Result> { }
}
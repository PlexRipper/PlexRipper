using FluentResults;
using MediatR;

namespace Data.Contracts;

public class RemoveInaccessibleServersCommand : IRequest<Result> { }
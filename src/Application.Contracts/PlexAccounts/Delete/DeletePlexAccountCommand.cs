using FluentResults;
using MediatR;

namespace Application.Contracts;

public record DeletePlexAccountCommand(int Id) : IRequest<Result>;
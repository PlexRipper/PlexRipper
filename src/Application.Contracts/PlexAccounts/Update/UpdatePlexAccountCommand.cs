using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

public record UpdatePlexAccountCommand(PlexAccount PlexAccount) : IRequest<Result>;
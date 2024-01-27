using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

public record UpdatePlexAccountCommand(PlexAccount PlexAccount, bool inspectServers = false) : IRequest<Result<PlexAccount>>;
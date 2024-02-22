using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

public record ValidateFolderPathsCommand(PlexMediaType MediaType = PlexMediaType.None) : IRequest<Result>;
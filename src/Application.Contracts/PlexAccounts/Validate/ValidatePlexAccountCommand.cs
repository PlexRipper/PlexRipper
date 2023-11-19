using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

/// <summary>
/// Validates the <see cref="PlexAccount"/> by calling the PlexAPI and confirming the PlexAccount can be used to login.
/// </summary>
/// <param name="PlexAccount">The <see cref="PlexAccount"/> to validate.</param>
public record ValidatePlexAccountCommand(PlexAccount PlexAccount) : IRequest<Result<PlexAccount>>;
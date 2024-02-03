using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

/// <summary>
/// Creates an <see cref="PlexAccount"/> in the Database and performs an QueueInspectPlexServerByPlexAccountIdJob().
/// </summary>
/// <param name="PlexAccount"> The <see cref="PlexAccount"/> to create. </param>
/// <returns>Returns the id of the created <see cref="PlexAccount"/></returns>
public record CreatePlexAccountCommand(PlexAccount PlexAccount) : IRequest<Result<int>>;
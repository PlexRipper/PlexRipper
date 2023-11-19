using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Application.Contracts;

/// <summary>
/// Retrieves all <see cref="PlexAccount"/>s with the included <see cref="PlexServer"/>s and <see cref="PlexLibrary"/>s.
/// </summary>
/// <param name="OnlyEnabled">Should only return enabled <see cref="PlexAccount">PlexAccounts</see>.</param>
/// <returns>A list of all <see cref="PlexAccount"/>s.</returns>
public record GetAllPlexAccountsQuery(bool OnlyEnabled = false) : IRequest<Result<List<PlexAccount>>>;
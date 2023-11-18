using FluentResults;
using MediatR;
using PlexRipper.Domain;

namespace Data.Contracts;

/// <summary>
///     Returns the <see cref="PlexAccount" /> by its id without any includes.
/// </summary>
public record GetPlexAccountByUsernameQuery(string Username, bool IncludePlexServers = false, bool IncludePlexLibraries = false)
    : IRequest<Result<PlexAccount>>;
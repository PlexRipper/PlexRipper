using FastEndpoints;
using FluentResults;
using Reaparr.Domain;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Sign in user with username and password and return user data with a Plex authentication token.
/// <remarks>NOTE: Plex "Managed" users do not work.</remarks>
/// <example>URL: https://plex.tv/api/v2/users/signin?X-Plex-Client-Identifier=Chrome</example>
/// </summary>
public record PlexSignInCommand(PlexAccount PlexAccount) : ICommand<Result<PlexAccount>>;

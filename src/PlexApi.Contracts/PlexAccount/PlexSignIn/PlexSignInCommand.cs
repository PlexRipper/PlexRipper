using FastEndpoints;

namespace Reaparr.PlexApi.Contracts;

/// <summary>
/// Sign in user with username and password and return user data with a Plex authentication token.
/// <remarks>NOTE: Plex "Managed" users do not work.</remarks>
/// <example>URL: https://plex.tv/api/v2/users/signin?X-Plex-Client-Identifier=Chrome</example>
/// </summary>
public record PlexSignInCommand : ICommand<Result<PlexSignInCommandResult>>
{
    public required string Username { get; init; }

    public required string Password { get; init; }

    public required string VerificationCode { get; init; }
}

using System.Text.Json.Serialization;

namespace Reaparr.Application;

public record CreatePlexAccountDTO
{
    public required string DisplayName { get; init; }

    public required string Username { get; init; }

    public required string Password { get; init; }

    public required bool IsEnabled { get; init; }

    public required bool IsMain { get; init; }

    public required bool IsValidated { get; init; }

    public required DateTime? ValidatedAt { get; init; }

    public required string Uuid { get; init; }

    public required long PlexId { get; init; }

    public required string Email { get; init; }

    public required string Title { get; init; }

    public required string ClientId { get; set; }

    public required bool Is2Fa { get; init; }

    /// <summary>
    /// The user has the option to provide their own token to authenticate with plex.tv.
    /// This is not the same as the auto filled AuthenticationToken when provided by the username and password
    /// </summary>
    public required string CustomAuthenticationToken { get; init; }

    public required string AuthenticationToken { get; init; }
}

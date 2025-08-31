using System.Text.Json.Serialization;

namespace Reaparr.Application.Contracts;

public class PlexAccountDTO
{
    public required int Id { get; set; }

    public required string DisplayName { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public required bool IsEnabled { get; set; }

    public required bool IsMain { get; set; }

    public required bool IsValidated { get; set; }

    public required DateTime? ValidatedAt { get; set; }

    public required string Uuid { get; set; }

    public required long PlexId { get; set; }

    public required string Email { get; set; }

    public required string Title { get; set; }

    public required bool HasPassword { get; set; }

    /// <summary>
    /// The user has the option to provide their own token to authenticate with plex.tv.
    /// This is not the same as the auto filled AuthenticationToken when provided by the username and password
    /// </summary>
    [JsonPropertyName("authenticationToken")]
    public required string CustomAuthenticationToken { get; set; }

    [JsonPropertyName("apiAuthenticationToken")]
    public required string AuthenticationToken { get; set; }

    public required string ClientId { get; set; }

    public required string VerificationCode { get; set; }

    public required bool Is2Fa { get; set; }

    public required List<int> PlexServerAccess { get; set; }
    public required List<int> PlexLibraryAccess { get; set; }
}

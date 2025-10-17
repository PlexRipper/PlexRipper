namespace Reaparr.Application.Contracts;

public record PlexAccountDTO
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
    /// The user can provide their own token to authenticate with plex.tv.
    /// This is different from the <see cref="AuthenticationToken"/> when provided by the username and password
    /// </summary>
    public required string CustomAuthenticationToken { get; set; }

    /// <summary>
    /// The authentication token provided by Plex when logging in with username and password.
    /// </summary>
    public required string AuthenticationToken { get; init; }

    public required string ClientId { get; set; }

    public required string VerificationCode { get; set; }

    public required bool Is2Fa { get; set; }

    public required List<int> PlexServerAccess { get; set; }
    public required List<int> PlexLibraryAccess { get; set; }
}

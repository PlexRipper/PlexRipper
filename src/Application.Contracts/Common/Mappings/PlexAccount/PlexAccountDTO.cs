namespace Application.Contracts;

public class PlexAccountDTO
{
    public int Id { get; set; }

    public string DisplayName { get; set; }

    public string Username { get; set; }

    public string Password { get; set; }

    public bool IsEnabled { get; set; }

    public bool IsMain { get; set; }

    public bool IsValidated { get; set; }

    public DateTime ValidatedAt { get; set; }

    public string Uuid { get; set; }

    public long PlexId { get; set; }

    public string Email { get; set; }

    public string Title { get; set; }

    public bool HasPassword { get; set; }

    public string AuthenticationToken { get; set; }

    public string ClientId { get; set; }

    public string VerificationCode { get; set; }

    public bool Is2Fa { get; set; }

    public List<PlexServerAccessDTO> PlexServerAccess { get; set; }
}
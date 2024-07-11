using FluentResults;

namespace Application.Contracts;

public class AuthPin
{
    #region Properties

    public List<PlexError> Errors { get; set; }

    public int Id { get; set; }

    public string Code { get; set; }

    public bool Trusted { get; set; }

    public string ClientIdentifier { get; set; }

    public AuthPinLocation Location { get; set; }

    public int ExpiresIn { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public string AuthToken { get; set; }

    public string NewRegistration { get; set; }

    #endregion
}

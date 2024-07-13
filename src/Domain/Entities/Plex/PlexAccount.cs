using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PlexRipper.Domain;

/// <summary>
/// The Plex Account entity in the database.
/// </summary>
public class PlexAccount : BaseEntity
{
    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="PlexAccount"/> class.
    /// </summary>
    public PlexAccount() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="PlexAccount"/> class.
    /// </summary>
    /// <param name="username">The username to use.</param>
    /// <param name="password">The password to use.</param>
    [SetsRequiredMembers]
    public PlexAccount(string username, string password)
    {
        Username = username;
        Password = password;
        IsEnabled = true;
    }

    public PlexAccount(string username, string password, string clientId, string verificationCode = "")
    {
        Username = username;
        Password = password;
        ClientId = clientId;
        VerificationCode = verificationCode;
    }

    #endregion

    #region Properties

    [Column(Order = 1)]
    public required string DisplayName { get; set; }

    [Column(Order = 2)]
    public required string Username { get; set; }

    [Column(Order = 3)]
    public required string Password { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexAccount"/> is enabled and that it should be used for downloading media.
    /// </summary>
    [Column(Order = 4)]
    public required bool IsEnabled { get; set; }

    /// <summary>
    /// Gets or sets whether this <see cref="PlexAccount"/> has been validated against the Plex API and contain valid credentials.
    /// </summary>
    [Column(Order = 5)]
    public required bool IsValidated { get; set; }

    [Column(Order = 6)]
    public required DateTime ValidatedAt { get; set; }

    [Column(Order = 7)]
    public required long PlexId { get; set; }

    [Column(Order = 8)]
    public required string Uuid { get; set; }

    /// <summary>
    /// The unique client identifier used for all PlexApi communication.
    /// </summary>
    [Column(Order = 9)]
    public required string ClientId { get; set; }

    [Column(Order = 10)]
    public required string Title { get; set; }

    [Column(Order = 11)]
    public required string Email { get; set; }

    [Column(Order = 12)]
    public required bool HasPassword { get; set; }

    /// <summary>
    /// The general plex authentication token used to retrieve account data such as the <see cref="PlexServer" />s the
    /// account has access to.
    /// </summary>
    public required string AuthenticationToken { get; set; }

    /// <summary>
    /// If this is a main account then it will get a lower priority when downloading media which a non-main account also has access to.
    /// </summary>
    public required bool IsMain { get; set; }

    #region Relationships

    /// <summary>
    /// The associated <see cref="PlexServer"/> this <see cref="PlexAccount"/> has access to.
    /// </summary>
    public List<PlexAccountServer> PlexAccountServers { get; set; } = new();

    /// <summary>
    /// The associated <see cref="PlexLibrary"/> this <see cref="PlexAccount"/> has access to.
    /// </summary>
    public List<PlexAccountLibrary> PlexAccountLibraries { get; set; } = new();

    #endregion

    #region Helpers

    [NotMapped]
    public List<PlexServer> PlexServers => PlexAccountServers.Select(x => x.PlexServer).ToList();

    /// <summary>
    /// Gets or sets whether this <see cref="PlexAccount"/> is 2FA protected.
    /// </summary>
    [NotMapped]
    public required bool Is2Fa { get; set; }

    /// <summary>
    /// The verification code given by the user if 2FA is enabled.
    /// </summary>
    [NotMapped]
    public required string VerificationCode { get; set; }

    #endregion

    #endregion
}

using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexAccountServer
{
    #region Relationships

    [Column(Order = 0)]
    public required int PlexAccountId { get; init; }

    public PlexAccount? PlexAccount { get; init; }

    [Column(Order = 1)]
    public required int PlexServerId { get; init; }

    public PlexServer? PlexServer { get; init; }

    #endregion

    #region Properties

    [Column(Order = 2)]
    public required string AuthToken { get; set; }

    [Column(Order = 3)]
    public required DateTime AuthTokenCreationDate { get; set; }

    #endregion
}

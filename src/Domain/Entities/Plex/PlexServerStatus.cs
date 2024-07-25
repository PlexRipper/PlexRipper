using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexServerStatus : BaseEntity
{
    #region Properties

    [Column(Order = 1)]
    public required bool IsSuccessful { get; init; }

    [Column(Order = 2)]
    public required int StatusCode { get; init; }

    [Column(Order = 3)]
    public required string StatusMessage { get; init; }

    [Column(Order = 4)]
    public required DateTime LastChecked { get; init; }

    #endregion

    #region Relationships

    public PlexServer? PlexServer { get; set; }

    [Column(Order = 5)]
    public required int PlexServerId { get; set; }

    public PlexServerConnection? PlexServerConnection { get; set; }

    [Column(Order = 6)]
    public required int PlexServerConnectionId { get; set; }

    #endregion
}

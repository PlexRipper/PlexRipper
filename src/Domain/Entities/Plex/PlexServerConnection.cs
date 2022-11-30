using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

public class PlexServerConnection : BaseEntity
{
    #region Properties

    [Column(Order = 1)]
    public string Protocol { get; set; }

    [Column(Order = 2)]
    public string Address { get; set; }

    [Column(Order = 3)]
    public int Port { get; set; }

    [Column(Order = 4)]
    public bool Local { get; set; }

    [Column(Order = 5)]
    public bool Relay { get; set; }

    [Column(Order = 6)]
    public bool IPv6 { get; set; }

    #endregion

    #region Relationships

    public PlexServer PlexServer { get; set; }

    public int PlexServerId { get; set; }

    #endregion

    #region Helpers

    public Uri Uri => new($"{Protocol}://{Address}:{Port}");

    #endregion
}
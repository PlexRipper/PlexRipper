using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain;

/// <summary>
/// Every <see cref="PlexServer"/> might have different ways to setup a connection through various domains or ip addresses.
/// All possible connections are stored through the use of <see cref="PlexServerConnection">PlexServerConnections</see>.
/// </summary>
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

    public List<PlexServerStatus> PlexServerStatus { get; set; }

    #endregion

    #region Helpers

    [NotMapped]
    public Uri Uri => new($"{Protocol}://{Address}:{Port}", UriKind.Absolute);

    [NotMapped]
    public string Url => Uri.ToString();

    #endregion

    #region Operators

    public static bool operator ==(PlexServerConnection left, PlexServerConnection right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(PlexServerConnection left, PlexServerConnection right)
    {
        return !Equals(left, right);
    }

    #endregion


    #region Equality

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Protocol, Address, Port, Local, Relay, IPv6, PlexServerId);
    }


    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;

        return Equals((PlexServerConnection)obj);
    }

    protected bool Equals(PlexServerConnection other)
    {
        return Protocol == other.Protocol && Address == other.Address && Port == other.Port && Local == other.Local && Relay == other.Relay &&
               IPv6 == other.IPv6 && PlexServerId == other.PlexServerId;
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[ServerId: {PlexServerId} - Url: {Url} - Local: {Local} - Relay: {Relay} - IPv6: {IPv6}]";
    }
}
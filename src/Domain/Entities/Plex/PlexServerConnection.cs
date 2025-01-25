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
    public required string Protocol { get; init; }

    [Column(Order = 2)]
    public required string Address { get; init; }

    [Column(Order = 3)]
    public required int Port { get; init; }

    [Column(Order = 4)]
    public required string Url { get; init; }

    [Column(Order = 5)]
    public required bool Local { get; init; }

    /// <summary>
    /// Gets whether this connection is relayed through Plex servers?
    /// </summary>
    [Column(Order = 6)]
    public required bool Relay { get; init; }

    [Column(Order = 7)]
    public required bool IPv4 { get; init; }

    [Column(Order = 8)]
    public required bool IPv6 { get; init; }

    /// <summary>
    /// Is this a custom connection created by the user.
    /// </summary>
    [Column(Order = 9)]
    public required bool IsCustom { get; init; }

    #endregion

    #region Relationships

    public PlexServer? PlexServer { get; init; }

    public required int PlexServerId { get; set; }

    public List<PlexServerStatus> PlexServerStatus { get; init; } = [];

    #endregion

    #region Helpers

    [NotMapped]
    public PlexServerStatus? LatestConnectionStatus => PlexServerStatus.FirstOrDefault();

    [NotMapped]
    public bool IsOnline => LatestConnectionStatus?.IsSuccessful ?? false;

    [NotMapped]
    public bool IsPlexTvConnection => Url.Contains(".plex.direct");

    public string GetDownloadUrl(string fileLocationUrl, string token) =>
        $"{Url}{fileLocationUrl}?X-Plex-Token={token}";

    public PlexConnectionTypes Type
    {
        get
        {
            if (IsCustom)
                return PlexConnectionTypes.Custom;

            if (Local)
                return PlexConnectionTypes.Local;

            if (IsPlexTvConnection)
                return PlexConnectionTypes.PlexRelay;

            return PlexConnectionTypes.Public;
        }
    }

    #endregion

    #region Operators

    public static bool operator ==(PlexServerConnection left, PlexServerConnection right) => Equals(left, right);

    public static bool operator !=(PlexServerConnection left, PlexServerConnection right) => !Equals(left, right);

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Protocol, Address, Port, Url, Local, Relay, IPv4, IPv6);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        return obj.GetType() == GetType() && Equals((PlexServerConnection)obj);
    }

    protected bool Equals(PlexServerConnection other) =>
        Protocol == other.Protocol
        && Address == other.Address
        && Port == other.Port
        && Url == other.Url
        && Local == other.Local
        && Relay == other.Relay
        && IPv6 == other.IPv6;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"[ServerId: {PlexServerId} - Url: {Url} - Local: {Local} - Relay: {Relay} - IPv6: {IPv6}]";
}

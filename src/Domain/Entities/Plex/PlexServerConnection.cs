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
    public required string Protocol { get; set; }

    [Column(Order = 2)]
    public required string Address { get; set; }

    [Column(Order = 3)]
    public required int Port { get; set; }

    [Column(Order = 4)]
    public required string Uri { get; init; }

    [Column(Order = 5)]
    public required bool Local { get; init; }

    [Column(Order = 6)]
    public required bool Relay { get; init; }

    [Column(Order = 7)]
    public required bool IPv4 { get; init; }

    [Column(Order = 8)]
    public required bool IPv6 { get; init; }

    /// <summary>
    /// The port fix is when we don't use the port when Address is a domain name.
    /// This seems to work in most cases where a domain name with the port appended will result in failed network requests.
    /// <remarks> This is set in the "PlexApiMappingProfile" when the data is received from the Plex API.</remarks>
    /// </summary>
    [Column(Order = 9)]
    public required bool PortFix { get; init; }

    #endregion

    #region Relationships

    public PlexServer? PlexServer { get; init; }

    public required int PlexServerId { get; set; }

    public List<PlexServerStatus> PlexServerStatus { get; init; } = new();

    #endregion

    #region Helpers

    [NotMapped]
    public string Url
    {
        get
        {
            if (!PortFix)
                return Uri;

            var urlBuilder = new UriBuilder(Protocol, Address) { Port = Port };
            return urlBuilder.ToString().TrimEnd('/');
        }
    }

    [NotMapped]
    public string Name => $"Connection: ({Url})";

    [NotMapped]
    public PlexServerStatus? LatestConnectionStatus => PlexServerStatus.FirstOrDefault();

    [NotMapped]
    public bool IsPlexTvConnection => Uri.Contains(".plex.direct");

    public string GetThumbUrl(string thumbPath)
    {
        var uri = new Uri(Url + thumbPath);
        return $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}";
    }

    public string GetDownloadUrl(string fileLocationUrl, string token) =>
        $"{Url}{fileLocationUrl}?X-Plex-Token={token}";

    #endregion

    #region Operators

    public static bool operator ==(PlexServerConnection left, PlexServerConnection right) => Equals(left, right);

    public static bool operator !=(PlexServerConnection left, PlexServerConnection right) => !Equals(left, right);

    #endregion

    #region Equality

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Protocol, Address, Port, Local, Relay, IPv6, PlexServerId);

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
        && Local == other.Local
        && Relay == other.Relay
        && IPv6 == other.IPv6
        && PlexServerId == other.PlexServerId;

    #endregion

    /// <inheritdoc/>
    public override string ToString() =>
        $"[ServerId: {PlexServerId} - Url: {Url} - {nameof(PortFix)}: {PortFix} - Local: {Local} - Relay: {Relay} - IPv6: {IPv6}]";
}

using PlexRipper.Domain;

namespace Application.Contracts;

public class PlexServerConnectionDTO
{
    public required int Id { get; set; }

    public required string Protocol { get; set; }

    public required string Address { get; set; }

    public required int Port { get; set; }

    public required bool IsCustom { get; set; }

    public required bool Local { get; set; }

    public required bool Relay { get; set; }

    // ReSharper disable once InconsistentNaming
    public required bool IPv4 { get; set; }

    // ReSharper disable once InconsistentNaming
    public required bool IPv6 { get; set; }

    /// <summary>
    /// Gets whether the front-end should use this connection to connect to the server, always 1 connection is chosen.
    /// </summary>
    public required bool ChosenConnection { get; set; }

    public required int PlexServerId { get; set; }

    public required string Url { get; set; }

    public required bool IsPlexTvConnection { get; set; }

    public required PlexConnectionTypes Type { get; set; }

    public required PlexServerStatusDTO? LatestConnectionStatus { get; set; }
}

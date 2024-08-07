namespace Application.Contracts;

public class PlexServerConnectionDTO
{
    #region Properties

    public required int Id { get; set; }

    public required string Protocol { get; set; }

    public required string Address { get; set; }

    public required int Port { get; set; }

    public required bool Local { get; set; }

    public required bool Relay { get; set; }

    // ReSharper disable once InconsistentNaming
    public required bool IPv4 { get; set; }

    // ReSharper disable once InconsistentNaming
    public required bool IPv6 { get; set; }

    public required bool PortFix { get; set; }

    public required int PlexServerId { get; set; }

    public required string Url { get; set; }

    public required string Uri { get; set; }

    public required bool IsPlexTvConnection { get; set; }

    public required List<PlexServerStatusDTO> ServerStatusList { get; set; }

    public required PlexServerStatusDTO? LatestConnectionStatus { get; set; }

    /// <summary>
    /// Added as a progress container for the front-end
    /// </summary>
    public required ServerConnectionCheckStatusProgressDTO? Progress { get; set; }

    #endregion
}

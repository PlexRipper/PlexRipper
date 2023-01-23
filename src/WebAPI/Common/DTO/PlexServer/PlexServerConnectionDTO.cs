namespace PlexRipper.WebAPI.Common.DTO;

public class PlexServerConnectionDTO
{
    #region Properties

    public int Id { get; set; }


    public string Protocol { get; set; }


    public string Address { get; set; }


    public int Port { get; set; }


    public bool Local { get; set; }


    public bool Relay { get; set; }


    public bool IPv4 { get; set; }


    public bool IPv6 { get; set; }


    public bool PortFix { get; set; }


    public int PlexServerId { get; set; }


    public string Url { get; set; }


    public PlexServerStatusDTO LatestConnectionStatus { get; set; }

    /// <summary>
    /// Added as a progress container for the front-end
    /// </summary>

    public ServerConnectionCheckStatusProgressDTO Progress { get; set; }

    #endregion
}
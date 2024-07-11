namespace PlexRipper.PlexApi.Api;

public class ServerIdentityResponse
{
    public ServerIdentityResponseMediaContainer MediaContainer { get; set; }
}

public class ServerIdentityResponseMediaContainer
{
    public int Size { get; set; }

    public bool Claimed { get; set; }

    public string MachineIdentifier { get; set; }

    public string Version { get; set; }
}

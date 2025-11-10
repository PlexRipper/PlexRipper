namespace Reaparr.PlexApi.Contracts;

public record PlexServerAccessDTO
{
    public required PlexServer PlexServer { get; set; }

    public required ServerAccessTokenDTO AccessToken { get; set; }
}

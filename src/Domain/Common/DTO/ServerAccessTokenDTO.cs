namespace PlexRipper.Domain;

public record ServerAccessTokenDTO
{
    public required int PlexAccountId { get; set; }

    public required string MachineIdentifier { get; set; }

    public required string AccessToken { get; set; }
}

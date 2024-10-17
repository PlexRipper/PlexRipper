namespace PlexApi.Contracts;

public record ServerAccessTokenDTO
{
    public required int PlexAccountId { get; init; }

    public required string MachineIdentifier { get; init; }

    public required string AccessToken { get; set; }
}

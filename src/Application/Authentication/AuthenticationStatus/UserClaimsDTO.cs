namespace PlexRipper.Application;

public record UserClaimsDTO
{
    public required bool IsLoggedIn { get; init; }

    public required string UserName { get; init; }

    public required List<string> Claims { get; init; }
}

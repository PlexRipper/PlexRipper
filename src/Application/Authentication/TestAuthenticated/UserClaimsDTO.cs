namespace PlexRipper.Application;

public record UserClaimsDTO()
{
    public bool IsLoggedIn { get; init; }

    public string UserName { get; init; }

    public IEnumerable<object> Claims { get; init; }
}

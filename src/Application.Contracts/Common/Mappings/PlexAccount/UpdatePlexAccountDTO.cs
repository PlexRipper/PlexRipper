namespace Application.Contracts;

public class UpdatePlexAccountDTO
{
    public required int Id { get; set; }

    public required string DisplayName { get; set; }

    public required string Username { get; set; }

    public required string Password { get; set; }

    public required bool IsEnabled { get; set; }

    public required bool IsMain { get; set; }

    public required bool IsAuthTokenMode { get; init; }
}

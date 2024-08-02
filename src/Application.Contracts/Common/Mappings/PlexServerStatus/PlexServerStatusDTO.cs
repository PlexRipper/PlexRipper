namespace Application.Contracts;

public class PlexServerStatusDTO
{
    public required int Id { get; set; }

    public required int StatusCode { get; set; }

    public required bool IsSuccessful { get; set; }

    public required string StatusMessage { get; set; }

    public required DateTime LastChecked { get; set; }

    public required int PlexServerId { get; set; }

    public required int PlexServerConnectionId { get; set; }
}

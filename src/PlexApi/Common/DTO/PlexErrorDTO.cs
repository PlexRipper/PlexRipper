namespace PlexRipper.PlexApi;

public record PlexErrorDTO
{
    public required int Code { get; init; }

    public required string Message { get; init; }

    public required int Status { get; init; }
}

namespace Application.Contracts;

public record ServerConnectionCheckStatusProgressDTO
{
    public required int PlexServerId { get; set; }

    public required int PlexServerConnectionId { get; set; }

    public required int RetryAttemptIndex { get; init; }

    public required int RetryAttemptCount { get; init; }

    public required int TimeToNextRetry { get; init; }

    public required int StatusCode { get; init; }

    public required bool ConnectionSuccessful { get; init; }

    public required bool Completed { get; init; }

    public required string Message { get; init; }
}

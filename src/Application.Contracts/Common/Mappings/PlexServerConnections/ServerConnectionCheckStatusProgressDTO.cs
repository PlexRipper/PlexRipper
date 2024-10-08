namespace Application.Contracts;

public class ServerConnectionCheckStatusProgressDTO
{
    public int PlexServerId { get; set; }

    public int PlexServerConnectionId { get; set; }

    public int RetryAttemptIndex { get; init; }

    public int RetryAttemptCount { get; init; }

    public int TimeToNextRetry { get; init; }

    public int StatusCode { get; init; }

    public bool ConnectionSuccessful { get; init; }

    public bool Completed { get; init; }

    public string Message { get; init; }
}

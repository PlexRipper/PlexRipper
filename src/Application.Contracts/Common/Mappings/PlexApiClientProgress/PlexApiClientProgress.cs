namespace Application.Contracts;

public class PlexApiClientProgress
{
    public required int RetryAttemptIndex { get; init; }

    public required int RetryAttemptCount { get; init; }

    public required int TimeToNextRetry { get; init; }

    public required int StatusCode { get; init; }

    public required bool ConnectionSuccessful { get; init; }

    public required bool Completed { get; init; }

    public required string Message { get; init; }

    public required string ErrorMessage { get; set; }
}

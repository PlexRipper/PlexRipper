namespace Application.Contracts;

public class PlexApiClientProgress
{
    #region Properties

    public required int RetryAttemptIndex { get; set; }

    public required int RetryAttemptCount { get; set; }

    public required int TimeToNextRetry { get; set; }

    public required int StatusCode { get; set; }

    public required bool ConnectionSuccessful { get; set; }

    public required bool Completed { get; set; }

    public required string Message { get; set; }

    public string ErrorMessage { get; set; }

    #endregion
}

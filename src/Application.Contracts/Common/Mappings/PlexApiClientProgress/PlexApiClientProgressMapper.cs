using PlexRipper.Domain;

namespace Application.Contracts;

public static class PlexApiClientProgressMapper
{
    #region ToDTO

    public static ServerConnectionCheckStatusProgress ToServerConnectionCheckStatusProgress(
        this PlexApiClientProgress source,
        PlexServerConnection plexServerConnection
    ) =>
        new()
        {
            RetryAttemptIndex = source.RetryAttemptIndex,
            RetryAttemptCount = source.RetryAttemptCount,
            TimeToNextRetry = source.TimeToNextRetry,
            StatusCode = source.StatusCode,
            ConnectionSuccessful = source.ConnectionSuccessful,
            Completed = source.Completed,
            Message = source.Message,
            PlexServerConnection = plexServerConnection,
        };

    #endregion
}

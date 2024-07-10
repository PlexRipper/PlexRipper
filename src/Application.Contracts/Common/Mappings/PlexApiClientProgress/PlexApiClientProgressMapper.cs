using PlexRipper.Domain;

namespace Application.Contracts;

public static partial class PlexApiClientProgressMapper
{
    #region ToDTO

    public static ServerConnectionCheckStatusProgress ToServerConnectionCheckStatusProgress(
        this PlexApiClientProgress source
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
            PlexServerConnection = new PlexServerConnection(),
        };

    #endregion
}

using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.Application;

internal static class HttpRequestRetryProgressMapper
{
    public static ServerConnectionCheckStatusProgress ToServerConnectionCheckStatusProgress(
        this HttpRequestRetryProgress source,
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
}

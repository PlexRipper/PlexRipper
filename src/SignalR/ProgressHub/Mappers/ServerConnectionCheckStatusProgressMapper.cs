using Reaparr.Application.Contracts;
using Reaparr.SignalR.Contracts;

namespace Reaparr.SignalR;

internal static class ServerConnectionCheckStatusProgressMapper
{
    public static ServerConnectionCheckStatusProgressDTO ToDTO(this ServerConnectionCheckStatusProgress source) =>
        new()
        {
            PlexServerId = source.PlexServerId,
            PlexServerConnectionId = source.PlexServerConnectionId,
            RetryAttemptIndex = source.RetryAttemptIndex,
            RetryAttemptCount = source.RetryAttemptCount,
            TimeToNextRetry = source.TimeToNextRetry,
            StatusCode = source.StatusCode,
            ConnectionSuccessful = source.ConnectionSuccessful,
            Completed = source.Completed,
            Message = source.Message,
        };
}

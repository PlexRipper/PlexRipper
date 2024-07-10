using PlexRipper.Domain;

namespace Application.Contracts;

public class ServerConnectionCheckStatusProgress
{
    public int PlexServerId => PlexServerConnection.PlexServerId;

    public int PlexServerConnectionId => PlexServerConnection.Id;

    public required int RetryAttemptIndex { get; set; }

    public required int RetryAttemptCount { get; set; }

    public required int TimeToNextRetry { get; set; }

    public required int StatusCode { get; set; }

    public required bool ConnectionSuccessful { get; set; }

    public required bool Completed { get; set; }

    public required string Message { get; set; }

    public required PlexServerConnection PlexServerConnection { get; set; }

    public override string ToString() =>
        $"{nameof(ServerConnectionCheckStatusProgress)}: "
        + $"ConnectionId: {PlexServerConnectionId} - "
        + $"ServerId: {PlexServerId} - "
        + $"({RetryAttemptIndex} of {RetryAttemptCount}) -"
        + $" Completed: {Completed} -"
        + $" Status: {StatusCode}"
        + $" Message: {Message}";
}

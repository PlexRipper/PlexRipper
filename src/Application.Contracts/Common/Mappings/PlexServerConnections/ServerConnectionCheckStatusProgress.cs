using PlexRipper.Domain;

namespace Application.Contracts;

public class ServerConnectionCheckStatusProgress
{
    public int PlexServerId => PlexServerConnection.PlexServerId;

    public int PlexServerConnectionId => PlexServerConnection.Id;

    public int RetryAttemptIndex { get; set; }

    public int RetryAttemptCount { get; set; }

    public int TimeToNextRetry { get; set; }

    public int StatusCode { get; set; }

    public bool ConnectionSuccessful { get; set; }

    public bool Completed { get; set; }

    public string Message { get; set; }

    public PlexServerConnection PlexServerConnection { get; set; }

    public override string ToString() => $"{nameof(ServerConnectionCheckStatusProgress)}: " +
                                         $"ConnectionId: {PlexServerConnectionId} - " +
                                         $"ServerId: {PlexServerId} - " +
                                         $"({RetryAttemptIndex} of {RetryAttemptCount}) -" +
                                         $" Completed: {Completed} -" +
                                         $" Status: {StatusCode}" +
                                         $" Message: {Message}";
}
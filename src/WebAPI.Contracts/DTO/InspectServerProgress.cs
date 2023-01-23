using PlexRipper.Domain;

namespace WebAPI.Contracts;

public class InspectServerProgress
{
    public int PlexServerId { get; set; }

    public int RetryAttemptIndex { get; set; }

    public int RetryAttemptCount { get; set; }

    public int TimeToNextRetry { get; set; }

    public int StatusCode { get; set; }

    public bool ConnectionSuccessful { get; set; }

    public bool Completed { get; set; }

    public string Message { get; set; }

    public bool AttemptingApplyDNSFix { get; set; }

    public PlexServerConnection PlexServerConnection { get; set; }
}
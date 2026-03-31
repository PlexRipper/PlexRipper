namespace Reaparr.PlexApi.Contracts;

public record TranscodeDecisionRequest
{
    public TranscodeDecisionRequest(string metaDataPath)
    {
        MetaDataPath = metaDataPath;
    }

    public string MetaDataPath { get; init; }

    public string ClientIdentifier { get; } = Guid.NewGuid().ToString("N")[..25];

    public string PlexSessionId { get; } = Guid.NewGuid().ToString("N")[..24];

    public string TranscodeSessionId { get; } = Guid.NewGuid().ToString("N")[..24];
}

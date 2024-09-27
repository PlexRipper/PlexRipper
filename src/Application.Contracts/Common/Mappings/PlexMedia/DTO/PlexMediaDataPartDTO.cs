namespace Application.Contracts;

public class PlexMediaDataPartDTO
{
    public required string ObfuscatedFilePath { get; init; }

    public required int Duration { get; init; }

    public required string File { get; init; }

    public required long Size { get; init; }

    public required string Container { get; init; }

    public required string VideoProfile { get; init; }
}

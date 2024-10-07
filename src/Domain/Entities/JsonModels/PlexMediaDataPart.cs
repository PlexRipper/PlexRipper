namespace PlexRipper.Domain;

public class PlexMediaDataPart
{
    public string ObfuscatedFilePath { get; init; } = string.Empty;
    public int Duration { get; init; }
    public string File { get; set; } = string.Empty;
    public long Size { get; init; }
    public string Container { get; init; } = string.Empty;
    public string VideoProfile { get; init; } = string.Empty;
    public string AudioProfile { get; init; } = string.Empty;
    public string Indexes { get; init; } = string.Empty;
}

namespace PlexRipper.Domain;

public class PlexMediaDataPart
{
    public string ObfuscatedFilePath { get; init; }
    public int Duration { get; init; }
    public string File { get; set; }
    public long Size { get; init; }
    public string Container { get; init; }
    public string VideoProfile { get; init; }
    public string AudioProfile { get; init; }
    public string Indexes { get; init; }
}

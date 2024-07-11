namespace PlexRipper.Domain;

public class PlexMediaDataPart
{
    #region Properties

    public string ObfuscatedFilePath { get; set; }

    public int Duration { get; set; }

    public string File { get; set; }

    public long Size { get; set; }

    public string Container { get; set; }

    public string VideoProfile { get; set; }

    // TODO Debate if streams should be stored, is it relevant for PlexRipper?
    // public Stream[] Stream { get; set; }

    public string AudioProfile { get; set; }

    public string HasThumbnail { get; set; }

    public string Indexes { get; set; }

    public bool? HasChapterTextStream { get; set; }

    #endregion
}

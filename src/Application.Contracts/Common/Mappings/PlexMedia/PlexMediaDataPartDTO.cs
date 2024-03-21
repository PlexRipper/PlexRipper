namespace Application.Contracts;

public class PlexMediaDataPartDTO
{
    #region Properties

    public string ObfuscatedFilePath { get; set; }

    public int Duration { get; set; }

    public string File { get; set; }

    public long Size { get; set; }

    public string Container { get; set; }

    public string VideoProfile { get; set; }

    #endregion
}
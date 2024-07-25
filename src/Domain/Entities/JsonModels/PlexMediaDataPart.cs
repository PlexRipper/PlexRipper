namespace PlexRipper.Domain
{
    public class PlexMediaDataPart
    {
        #region Properties

        public string ObfuscatedFilePath { get; init; }
        public int Duration { get; init; }
        public string File { get; set; }
        public long Size { get; init; }
        public string Container { get; init; }
        public string VideoProfile { get; init; }

        // TODO Debate if streams should be stored, is it relevant for PlexRipper?
        // public Stream[] Stream { get; set; }

        public string AudioProfile { get; init; }
        public string HasThumbnail { get; init; }
        public string Indexes { get; init; }
        public bool? HasChapterTextStream { get; init; }

        #endregion
    }
}

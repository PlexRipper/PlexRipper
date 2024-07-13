namespace PlexRipper.Domain
{
    public class PlexMediaDataPart
    {
        #region Properties

        public required string ObfuscatedFilePath { get; set; }
        public required int Duration { get; set; }
        public required string File { get; set; }
        public required long Size { get; set; }
        public required string Container { get; set; }
        public required string VideoProfile { get; set; }

        // TODO Debate if streams should be stored, is it relevant for PlexRipper?
        // public Stream[] Stream { get; set; }

        public required string AudioProfile { get; set; }
        public required string HasThumbnail { get; set; }
        public required string Indexes { get; set; }
        public bool? HasChapterTextStream { get; set; }

        #endregion
    }
}

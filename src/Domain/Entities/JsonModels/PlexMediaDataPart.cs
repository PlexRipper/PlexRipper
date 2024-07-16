namespace PlexRipper.Domain
{
    public class PlexMediaDataPart
    {
        #region Properties

        public required string ObfuscatedFilePath { get; init; }
        public required int Duration { get; init; }
        public required string File { get; set; }
        public required long Size { get; init; }
        public required string Container { get; init; }
        public required string VideoProfile { get; init; }

        // TODO Debate if streams should be stored, is it relevant for PlexRipper?
        // public Stream[] Stream { get; set; }

        public required string AudioProfile { get; init; }
        public required string HasThumbnail { get; init; }
        public required string Indexes { get; init; }
        public bool? HasChapterTextStream { get; init; }

        #endregion
    }
}
